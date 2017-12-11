﻿using Lib_K_Relay.GameData;
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
        private const double MOVEMENT_THRESHOLD_DISTANCE = 1;
        private static readonly TimeSpan TIMESPAN_RESTART_MOVEMENT = TimeSpan.FromSeconds(0.5);

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

        public void BeginMove(Point target)
        {
            // It's impossible to find a path that leads to an unwalkable tile
            if (!map.Tiles[target.X, target.Y].Walkable)
            {
                Plugin.Log("Destination tile is not walkable!");
                return;
            }

            // Cancel the previous movement task if there was any
            if (Moving)
            {
                moveCTS.Cancel();
                moveTask.Wait();
            }

            // Create a new movement task
            moveCTS = new CancellationTokenSource();
            moveTask = Task.Factory.StartNew(() =>
            {
                // Find the shortest path and simplify it
                IEnumerable<Point> shortestPath = FindShortestPath(target);

                // Abort if no path is found
                if (shortestPath == null || shortestPath.Count() == 0)
                {
                    Plugin.Log("Unable to find a path to the destination!");
                    return;
                }

                // Convert points to locations pointing to the center of the tile rather than the top-left corner
                IEnumerable<Location> converted = SimplifyPath(shortestPath).Select(x => new Location(x.X + 0.5f, x.Y + 0.5f));

                // Move along the path
                converted.ForEach(x =>
                {
                    if (moveCTS.IsCancellationRequested)
                    {
                        return;
                    }

                    MoveDirectlyToTarget(x);
                });

                if (!moveCTS.IsCancellationRequested)
                {
                    Plugin.Log("Arrived at the destination!");
                }
            }, moveCTS.Token);
        }

        public void CancelMove() => moveCTS.Cancel();

        private void MoveDirectlyToTarget(Location target)
        {
            // Get the current time and player position
            Location lastPlayerPos = client.PlayerData.Pos;
            DateTime dtLastMove = DateTime.Now;

            // Move towards the target location
            // Loop breaks when the target location is reached, when a task
            // cancellation is requested or when the client connection drops
            while (client.Connected && !moveCTS.IsCancellationRequested &&
                flash.SetMovementDirection(target.X - client.PlayerData.Pos.X, target.Y - client.PlayerData.Pos.Y, MOVEMENT_THRESHOLD_DISTANCE))
            {
                // Player has moved since the last iteration
                if (lastPlayerPos.X != client.PlayerData.Pos.X || lastPlayerPos.Y != client.PlayerData.Pos.Y)
                {
                    // Update the current time and player position
                    lastPlayerPos = client.PlayerData.Pos;
                    dtLastMove = DateTime.Now;
                }
                // Player hasn't moved for some time
                else if (DateTime.Now - dtLastMove >= TIMESPAN_RESTART_MOVEMENT)
                {
                    // Stop moving to reset the key states
                    flash.StopMovement();
                }
            }

            // Stop all movement
            flash.StopMovement();

            if (!moveCTS.IsCancellationRequested)
            {
                // Only jump if it's possible for the player to get stuck
                Point playerPos = client.GetPlayerLocationAsPoint();

                // Get all the surrounding tiles
                List<GameMapTile> surroundingTiles = new List<GameMapTile>();

                for (int y = (playerPos.Y > 0 ? -1 : 0); y <= (playerPos.Y < map.Height - 1 ? 1 : 0); y++)
                {
                    for (int x = (playerPos.X > 0 ? -1 : 0); x <= (playerPos.X < map.Width - 1 ? 1 : 0); x++)
                    {
                        if (y == 0 && x == 0)
                        {
                            continue;
                        }

                        surroundingTiles.Add(map.Tiles[playerPos.X + x, playerPos.Y + y]);
                    }
                }

                // If one of the surrounding tiles is unwalkable
                if (surroundingTiles.Exists(x => !x.Walkable))
                {
                    // Jump to the exact target location to avoid getting stuck
                    client.Jump(target);
                    Thread.Sleep(200);
                }
            }
        }

        private IEnumerable<Point> FindShortestPath(Point target)
        {
            LinkedList<GameMapTile> shortestPath = new SpatialAStar<GameMapTile>(map.Tiles).Search(client.GetPlayerLocationAsPoint(), target);
            return shortestPath?.Select(x => x.Location);
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