using Lib_K_Relay.GameData;
using Lib_K_Relay.GameData.DataStructures;
using Lib_K_Relay.Networking.Packets.DataObjects;
using Lib_K_Relay.Networking.Packets.Server;
using System.Collections.Generic;

namespace NtxBot
{
    public class GameMap
    {
        public readonly string Name;
        public readonly int Width;
        public readonly int Height;

        public GameMapTile[,] Tiles { get; private set; }
        public List<Entity> LivingEntities { get; private set; }

        public GameMap(MapInfoPacket mip)
        {
            Name = mip.Name;
            Width = mip.Width;
            Height = mip.Height;

            Tiles = new GameMapTile[Width, Height];

            // Initialize tiles to contain their coordinates
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    Tiles[x, y] = new GameMapTile(x, y);
                }
            }

            LivingEntities = new List<Entity>();
        }

        public void ProcessPacket(UpdatePacket p)
        {
            // Tiles
            p.Tiles.ForEach(x => Tiles[x.X, x.Y].SetTile(x.Type));

            // New objects
            foreach (Entity ent in p.NewObjs)
            {
                ObjectStructure objStruct = GameData.Objects.ByID(ent.ObjectType);

                // Objects with 0 maximum HP are immobile world elements
                // These objects are stored inside of the tiles on which they're standing
                if (objStruct.OccupySquare || (objStruct.MaxHP == 0 && !objStruct.Pet))
                {
                    // Get the tile coordinates
                    int x = (int)ent.Status.Position.X;
                    int y = (int)ent.Status.Position.Y;

                    // Add the object to the tile
                    Tiles[x, y].AddObject(objStruct);
                }
                // Add new living entities to the list
                else
                {
                    LivingEntities.Add(ent);
                }
            }

            // Drops
            foreach (int objId in p.Drops)
            {
                // Remove objects with the specified object ID from the list
                LivingEntities.RemoveAll(x => x.Status.ObjectId == objId);
            }
        }

        public void ProcessPacket(NewTickPacket p)
        {
            // Update statuses of living entities
            foreach (Status stat in p.Statuses)
            {
                LivingEntities.FindAll(x => x.Status.ObjectId == stat.ObjectId).ForEach(x => x.Status = stat);
            }
        }
    }
}