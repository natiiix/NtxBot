using Lib_K_Relay.Networking;
using Lib_K_Relay.GameData;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        //public void Run()
        //{
        //    while (client.Connected)
        //    {
        //        Plugin.Log("Uncovering...");
        //        UncoverPath();
        //        Task.Delay(200);

        //        int godIdx = map.LivingEntities.FindIndex(x => GameData.Objects.ByID(x.ObjectType).God);

        //        //if (godIdx < 0 || map.LivingEntities[godIdx].Status.Position.DistanceTo(client.PlayerData.Pos) < 20)
        //        //{
        //        //    Plugin.Log("God reached!");
        //        //    break;
        //        //}
        //    }

        //    Plugin.Log("Done running!");
        //}

        public void UncoverPath()
        {
            Point pathPos = FindNearestCoveredPath();

            if (pathPos == null)
            {
                Plugin.Log("No covered path available!");
                return;
            }

            moveEng.Move(pathPos);
        }

        private Point FindNearestCoveredPath()
        {
            List<Point> coveredPaths = new List<Point>();

            // Find walkable tiles with an uncovered tile next to them
            for (int y = 0; y < map.Height; y++)
            {
                for (int x = 0; x < map.Width; x++)
                {
                    if (map.Tiles[x, y].Walkable)
                    {
                        bool keepGoing = true;

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

            // Find the path closest to the player
            Point playerPos = client.GetPlayerLocationAsPoint();
            return coveredPaths.OrderBy(x => x.DistanceTo(playerPos)).First();
        }
    }
}