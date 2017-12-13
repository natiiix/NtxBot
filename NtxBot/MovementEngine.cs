using Lib_K_Relay.GameData;
using Lib_K_Relay.Networking;
using Lib_K_Relay.Networking.Packets.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NtxBot
{
    public class MovementEngine
    {
        private const double MOVEMENT_DISTANCE_THRESHOLD = 0.5;
        private static readonly TimeSpan TIME_BEFORE_STUCK = TimeSpan.FromSeconds(1);

        private Client client;
        private FlashClient flash;
        private GameMap map;

        private Task moveTask;
        private CancellationTokenSource moveCTS;

        public bool Moving { get => moveTask != null && moveTask.Status == TaskStatus.Running; }

        public MovementEngine(Client client, FlashClient flash, GameMap map)
        {
            this.client = client;
            this.flash = flash;
            this.map = map;

            moveTask = null;
        }

        public void CancelMove() => moveCTS?.Cancel();

        public void Move(Point target)
        {
            // It's impossible to find a path that leads to an unwalkable tile
            if (!map.Tiles[target.X, target.Y].Walkable)
            {
                Plugin.Log("Destination tile is not walkable!");
                return;
            }

            // Find the shortest path and simplify it
            IEnumerable<Point> shortestPath = map.FindShortestPath(client.GetPlayerLocationAsPoint(), target);

            // Abort if no path is found
            if (shortestPath == null || shortestPath.Count() == 0)
            {
                Plugin.Log("Unable to find a path to the destination!");
                return;
            }

            MoveDirectlyAlongPath(shortestPath);

            if (moveCTS == null || !moveCTS.IsCancellationRequested)
            {
                Plugin.Log("Arrived at the destination!");
            }
        }

        public void BeginMove(Point target)
        {
            // Cancel the previous movement task if there was any
            if (Moving)
            {
                moveCTS.Cancel();
                moveTask.Wait();
            }

            // Create a new movement task
            moveCTS = new CancellationTokenSource();
            moveTask = Task.Factory.StartNew(() => Move(target), moveCTS.Token);
        }

        public void MoveDirectlyToTarget(Location target)
        {
            // Get the current time and player position
            Location lastPlayerPos = client.PlayerData.Pos;
            DateTime dtLastMove = DateTime.Now;

            // Move towards the target location
            // Loop breaks when the target location is reached, when a task
            // cancellation is requested or when the client connection drops
            while (client.Connected && (moveCTS == null || !moveCTS.IsCancellationRequested))
            {
                Location playerPos = client.PlayerData.Pos;

                // Player has moved since the last iteration
                if (lastPlayerPos.X != playerPos.X || lastPlayerPos.Y != playerPos.Y)
                {
                    // Update the current time and player position
                    lastPlayerPos = playerPos;
                    dtLastMove = DateTime.Now;
                }
                // Player hasn't moved for some time
                else if (DateTime.Now - dtLastMove >= TIME_BEFORE_STUCK)
                {
                    //Plugin.Log("Stuck!");
                    //Plugin.Log("Player: " + playerPos.ToString());
                    //Plugin.Log("Target: " + target.ToString());
                    //throw new Exception("Stuck!");

                    // Stop moving to reset the key states
                    Plugin.Log("Resetting movement key states...");
                    flash.StopMovement();
                }

                // Set movement direction
                {
                    double xOffset = target.X - playerPos.X;
                    double yOffset = target.Y - playerPos.Y;

                    bool xAboveThreshold = Math.Abs(xOffset) > MOVEMENT_DISTANCE_THRESHOLD;
                    bool yAboveThreshold = Math.Abs(yOffset) > MOVEMENT_DISTANCE_THRESHOLD;

                    if (!xAboveThreshold && !yAboveThreshold)
                    {
                        break;
                    }

                    Point playerTile = (Point)playerPos;

                    bool xBlocked = xAboveThreshold && (
                        (xOffset < 0 && (!map.Tiles[playerTile.X - 1, playerTile.Y - 1].Walkable || !map.Tiles[playerTile.X - 1, playerTile.Y + 1].Walkable)) ||
                        (xOffset > 0 && (!map.Tiles[playerTile.X + 1, playerTile.Y - 1].Walkable || !map.Tiles[playerTile.X + 1, playerTile.Y + 1].Walkable)));

                    bool yBlocked = yAboveThreshold && (
                        (yOffset < 0 && (!map.Tiles[playerTile.X - 1, playerTile.Y - 1].Walkable || !map.Tiles[playerTile.X + 1, playerTile.Y - 1].Walkable)) ||
                        (yOffset > 0 && (!map.Tiles[playerTile.X - 1, playerTile.Y + 1].Walkable || !map.Tiles[playerTile.X + 1, playerTile.Y + 1].Walkable)));

                    flash.A = xOffset < 0 && (xAboveThreshold || yBlocked);
                    flash.D = xOffset > 0 && (xAboveThreshold || yBlocked);
                    flash.W = yOffset < 0 && (yAboveThreshold || xBlocked);
                    flash.S = yOffset > 0 && (yAboveThreshold || xBlocked);
                }
            }

            // Stop all movement
            flash.StopMovement();

            //if (moveCTS == null || !moveCTS.IsCancellationRequested)
            //{
            //    // Only jump if it's possible for the player to get stuck
            //    Point playerPos = client.GetPlayerLocationAsPoint();

            //    // Get all the surrounding tiles
            //    List<GameMapTile> surroundingTiles = new List<GameMapTile>();

            //    for (int y = (playerPos.Y > 0 ? -1 : 0); y <= (playerPos.Y < map.Height - 1 ? 1 : 0); y++)
            //    {
            //        for (int x = (playerPos.X > 0 ? -1 : 0); x <= (playerPos.X < map.Width - 1 ? 1 : 0); x++)
            //        {
            //            if (y == 0 && x == 0)
            //            {
            //                continue;
            //            }

            //            surroundingTiles.Add(map.Tiles[playerPos.X + x, playerPos.Y + y]);
            //        }
            //    }

            //    // If one of the surrounding tiles is unwalkable
            //    if (surroundingTiles.Exists(x => !x.Walkable))
            //    {
            //        // Jump to the exact target location to avoid getting stuck
            //        client.MoveUsingGoTo(target);
            //        Task.Delay(250);
            //    }
            //}
        }

        public void MoveDirectlyToTarget(Point target)
        {
            if (!map.Tiles[target.X, target.Y].Walkable)
            {
                Plugin.Log("Unable to move to an unwalkable tile!");
                return;
            }

            // Convert the point to a location pointing to the center of the tile rather than the top-left corner
            MoveDirectlyToTarget(new Location(target.X + 0.5f, target.Y + 0.5f));
        }

        public void MoveDirectlyAlongPath(IEnumerable<Point> pathNodes)
        {
            if (moveCTS != null && moveCTS.IsCancellationRequested)
            {
                return;
            }

            // Simplify the path by removing redundant nodes
            IEnumerable<Point> simplified = SimplifyPath(pathNodes);

            // Move along the path
            simplified.ForEach(x =>
            {
                if (moveCTS != null && moveCTS.IsCancellationRequested)
                {
                    return;
                }

                MoveDirectlyToTarget(x);
            });
        }

        // Removes unnecessary nodes from the path
        private static IEnumerable<Point> SimplifyPath(IEnumerable<Point> path)
        {
            if (path == null)
            {
                return null;
            }

            List<Point> simplifiedPath = new List<Point>();

            // Information about the previous node
            Point prevNode = path.First();
            Point prevDirection = null;

            for (int i = 1; i < path.Count(); i++)
            {
                // Information about the current node
                Point node = path.ElementAt(i);
                Point direction = node - prevNode;

                // If the path direction has changed, add the corner node to the simplified path
                if (i > 1 && !direction.Equals(prevDirection))
                {
                    simplifiedPath.Add(prevNode);
                }

                // End of iteration
                // This node is now going to be the previous node to the next day
                prevNode = node;
                prevDirection = direction;
            }

            // Add the last node to the simplified path
            simplifiedPath.Add(path.Last());

            return simplifiedPath;
        }
    }
}