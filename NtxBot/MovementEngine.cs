using Lib_K_Relay.GameData;
using Lib_K_Relay.Networking;
using Lib_K_Relay.Networking.Packets.DataObjects;
using Lib_K_Relay.Networking.Packets.Client;
using Lib_K_Relay.Networking.Packets.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lib_K_Relay.Networking.Packets;

namespace NtxBot
{
    public class MovementEngine
    {
        private static readonly TimeSpan TIME_BEFORE_KEY_RESET = TimeSpan.FromSeconds(1);
        private const double THRESHOLD_DISTANCE_UNSAFE = 0.5;
        private const double THRESHOLD_DISTANCE_SAFE = 0.9;
        private const double MAX_GOTO_DISTANCE = THRESHOLD_DISTANCE_UNSAFE;

        private Client client;
        private FlashClient flash;
        private GameMap map;

        private Task moveTask;

        public bool Moving { get => moveTask != null && moveTask.Status == TaskStatus.Running; }

        public MovementEngine(Client client, FlashClient flash, GameMap map)
        {
            this.client = client;
            this.flash = flash;
            this.map = map;

            moveTask = null;
        }

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
            Plugin.Log("Arrived at the destination!");
        }

        public void BeginMove(Point target)
        {
            // Wait for the previous movement task to complete
            if (Moving)
            {
                moveTask.Wait();
            }

            // Create a new movement task
            moveTask = Task.Factory.StartNew(() => Move(target));
        }

        public void MoveDirectlyToTarget(Location target, double thresholdDistance)
        {
            // Get the current time and player position
            Location lastPlayerPos = client.PlayerData.Pos;
            DateTime dtLastMove = DateTime.Now;

            // Move towards the target location
            // Loop breaks when the target location is reached, when a task
            // cancellation is requested or when the client connection drops
            while (client.Connected)
            {
                Location playerPos = client.PlayerData.Pos;

                // Player has moved since the last tick
                if (lastPlayerPos.X != playerPos.X || lastPlayerPos.Y != playerPos.Y)
                {
                    // Update the current time and player position
                    lastPlayerPos = playerPos;
                    dtLastMove = DateTime.Now;
                }
                // Player hasn't moved for too long
                else if (DateTime.Now - dtLastMove >= TIME_BEFORE_KEY_RESET)
                {
                    // Stop moving to reset the key states
                    Plugin.Log("Resetting movement key states...");
                    flash.StopMovement();

                    // If close enough to the target, use GOTO to jump to the exact location
                    if (playerPos.DistanceTo(target) <= MAX_GOTO_DISTANCE)
                    {
                        GotoPacket p = Packet.Create<GotoPacket>(PacketType.GOTO);

                        p.Location = target;
                        p.ObjectId = client.ObjectId;

                        client.SendToClient(p);
                        Task.Delay(100);
                        return;
                    }

                    // Reset the time of the last move to avoid flooding the log
                    dtLastMove = DateTime.Now;
                }

                // Set movement direction
                {
                    double xOffset = target.X - playerPos.X;
                    double yOffset = target.Y - playerPos.Y;

                    bool xAboveThreshold = Math.Abs(xOffset) > thresholdDistance;
                    bool yAboveThreshold = Math.Abs(yOffset) > thresholdDistance;

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

                    client.PlayerData.Speed = 30;

                    flash.Left = xOffset < 0 && (xAboveThreshold || yBlocked);
                    flash.Right = xOffset > 0 && (xAboveThreshold || yBlocked);
                    flash.Up = yOffset < 0 && (yAboveThreshold || xBlocked);
                    flash.Down = yOffset > 0 && (yAboveThreshold || xBlocked);
                }
            }

            // Stop all movement
            flash.StopMovement();
        }

        public void MoveDirectlyAlongPath(IEnumerable<Point> pathNodes)
        {
            // Simplify the path by removing redundant nodes
            IEnumerable<Point> simplified = SimplifyPath(pathNodes);

            // Move along the path
            simplified.ForEach(x =>
            {
                bool surroundingsWalkable =
                    map.Tiles[x.X - 1, x.Y].Walkable &&
                    map.Tiles[x.X + 1, x.Y].Walkable &&
                    map.Tiles[x.X, x.Y - 1].Walkable &&
                    map.Tiles[x.X, x.Y + 1].Walkable &&
                    map.Tiles[x.X - 1, x.Y - 1].Walkable &&
                    map.Tiles[x.X - 1, x.Y + 1].Walkable &&
                    map.Tiles[x.X + 1, x.Y - 1].Walkable &&
                    map.Tiles[x.X + 1, x.Y + 1].Walkable;

                MoveDirectlyToTarget((Location)x, surroundingsWalkable ? THRESHOLD_DISTANCE_SAFE : THRESHOLD_DISTANCE_UNSAFE);
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