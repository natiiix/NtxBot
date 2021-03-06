﻿using Lib_K_Relay.Networking;
using Lib_K_Relay.Networking.Packets.DataObjects;
using System.Collections.Generic;
using System.Linq;

namespace NtxBot
{
    public class AbyssBot
    {
        private const double BOSS_DISTANCE_MIN = 5;
        private const double BOSS_DISTANCE_MAX = 8;

        private Client client;
        private FlashClient flash;
        private GameMap map;

        private MovementEngine moveEng;

        public AbyssBot(Client client, FlashClient flash, GameMap map)
        {
            this.client = client;
            this.flash = flash;
            this.map = map;

            moveEng = new MovementEngine(client, flash, map);
        }

        public void Run()
        {
            if (map.Name != "Abyss of Demons")
            {
                Plugin.Log("You must be in an Abyss to use this command!");
                return;
            }

            if (map.QuestObject == null)
            {
                Plugin.Log("Cannot start the Abyss bot! Quest object is not available!");
                return;
            }

            Plugin.Log("Uncovering...");

            // Keep uncovering until the boss is reached or something fails
            while (client.Connected && UncoverPath())
            {
                Entity boss = map.QuestObject;

                if (boss == null || client.PlayerData.Pos.DistanceTo(boss.Status.Position) < BOSS_DISTANCE_MAX)
                {
                    break;
                }
            }

            Plugin.Log("Fighting the boss...");

            // Boss fight
            while (client.Connected)
            {
                Entity boss = map.QuestObject;

                if (boss == null)
                {
                    break;
                }

                // Follow the boss
                moveEng.MoveDirectlyAlongPath(FindPathToBossFight());
            }

            Plugin.Log("Done!");
        }

        public bool UncoverPath()
        {
            IEnumerable<Point> pathToUncover = FindNearestCoveredPath();

            if (pathToUncover == null || pathToUncover.Count() == 0)
            {
                Plugin.Log("No covered path available!");
                return false;
            }

            moveEng.MoveDirectlyAlongPath(pathToUncover);
            return true;
        }

        private IEnumerable<Point> FindNearestCoveredPath()
        {
            List<Point> coveredPaths = new List<Point>();

            // Find walkable tiles with a covered tile next to them
            for (int y = 0; y < map.Height; y++)
            {
                for (int x = 0; x < map.Width; x++)
                {
                    if (map.Tiles[x, y].Walkable)
                    {
                        bool keepGoing = true;

                        // Check the surrounding tiles for covered tiles
                        for (int yOffset = y == 0 ? 0 : -1; yOffset <= 1 && y + yOffset < map.Height && keepGoing; yOffset++)
                        {
                            for (int xOffset = x == 0 ? 0 : -1; xOffset <= 1 && x + xOffset < map.Width && keepGoing; xOffset++)
                            {
                                if (yOffset == 0 && xOffset == 0)
                                {
                                    continue;
                                }

                                if (!map.Tiles[x + xOffset, y + yOffset].Uncovered)
                                {
                                    coveredPaths.Add(map.Tiles[x, y].Location);
                                    keepGoing = false;
                                }
                            }
                        }
                    }
                }
            }

            // No covered path found
            if (coveredPaths.Count == 0)
            {
                return null;
            }

            Location bossPos = map.QuestObject.Status.Position;
            Location playerPos = client.PlayerData.Pos;
            Point playerTile = client.GetPlayerLocationAsPoint();

            // Order the paths by their distance from the boss and the player in ascending order
            IOrderedEnumerable<Point> pathsByDistance = coveredPaths.OrderBy(x =>
            {
                Location pathPos = (Location)x;
                return pathPos.DistanceSquaredTo(bossPos) + pathPos.DistanceSquaredTo(playerPos);
            });

            // Find the nearest covered path with a valid path leading to it
            foreach (Point x in pathsByDistance)
            {
                // Try to find a path to the target position
                IEnumerable<Point> path = map.FindShortestPath(playerTile, x);

                // If there is a path that leads to the target position return it
                if (path != null && path.Count() > 0)
                {
                    return path;
                }
            }

            // There is no covered path that can be uncovered
            return null;
        }

        private IEnumerable<Point> FindPathToBossFight()
        {
            Entity boss = map.QuestObject;

            if (boss == null)
            {
                return null;
            }

            // Get all the walkable tile within a reasonable distance of the boss
            Point bossTile = (Point)boss.Status.Position;
            IEnumerable<Point> usableTiles = map.GetWalkableTilesInDistanceFromTile(bossTile, BOSS_DISTANCE_MIN, BOSS_DISTANCE_MAX);

            if (usableTiles == null || usableTiles.Count() == 0)
            {
                return null;
            }

            // Order the tiles by their distance from the player in ascending order
            Location playerPos = client.PlayerData.Pos;
            //IOrderedEnumerable<Point> tilesClosestToPlayer = usableTiles.OrderBy(x => ((Location)x).DistanceTo(playerPos));
            IOrderedEnumerable<Point> tilesClosestToPlayer = usableTiles.OrderBy(x => System.Math.Abs(((Location)x).DistanceTo(playerPos) - 4.0));

            // Find the path to the closest possible tile
            Point playerTile = client.GetPlayerLocationAsPoint();
            foreach (Point tile in tilesClosestToPlayer)
            {
                IEnumerable<Point> path = map.FindShortestPath(playerTile, tile);

                // If the path is valid return it
                if (path != null && path.Count() > 0)
                {
                    return path;
                }
            }

            return null;
        }
    }
}