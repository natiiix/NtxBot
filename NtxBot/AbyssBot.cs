using Lib_K_Relay.Networking;
using Lib_K_Relay.GameData;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lib_K_Relay.Networking.Packets.DataObjects;

namespace NtxBot
{
    public class AbyssBot
    {
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
            if (map.QuestObject == null)
            {
                Plugin.Log("Cannot start the Abyss bot! Quest object is not available!");
                return;
            }

            Plugin.Log("Running...");

            // Keep uncovering until the boss is reached or something fails
            while (client.Connected && UncoverPath())
            {
                Entity boss = map.QuestObject;

                if (boss == null && client.PlayerData.Pos.DistanceTo(boss.Status.Position) < 16)
                {
                    break;
                }
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

            Point playerPos = client.GetPlayerLocationAsPoint();
            Location bossPos = map.QuestObject.Status.Position;

            // Order the paths by distance to the player in ascending order
            //IOrderedEnumerable<Point> pathsByDistance = coveredPaths.OrderBy(x => x.DistanceTo(playerPos));

            // Order the paths by distance to the boss in ascending order
            IOrderedEnumerable<Point> pathsByDistance = coveredPaths.OrderBy(x => ((Location)x).DistanceTo(bossPos));

            // Find the nearest covered path with a valid path leading to it
            foreach (Point x in pathsByDistance)
            {
                // Try to find a path to the target position
                IEnumerable<Point> path = map.FindShortestPath(playerPos, x);

                // If there is a path that leads to the target position return it
                if (path != null && path.Count() != 0)
                {
                    return path;
                }
            }

            // There is no covered path that can be uncovered
            return null;
        }
    }
}