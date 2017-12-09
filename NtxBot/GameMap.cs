using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib_K_Relay.GameData;
using Lib_K_Relay.Networking;
using Lib_K_Relay.Networking.Packets;
using Lib_K_Relay.Networking.Packets.DataObjects;
using Lib_K_Relay.Networking.Packets.Server;
using Lib_K_Relay.Networking.Packets.Client;

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
            LivingEntities = new List<Entity>();
        }

        private bool AreValidCoordinates(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;

        public ushort? GetTileType(int x, int y)
        {
            GameMapTile tile = Tiles[x, y];

            if (tile == null)
            {
                return null;
            }

            return tile.TileType;
        }

        public void ProcessPacket(UpdatePacket p)
        {
            // Tiles
            foreach (Tile tile in p.Tiles)
            {
                // Create the tile if it doesn't exist
                if (Tiles[tile.X, tile.Y] == null)
                {
                    Tiles[tile.X, tile.Y] = new GameMapTile(tile.Type);
                }
                // Tile exists
                // Modify the tile type
                else
                {
                    Tiles[tile.X, tile.Y].TileType = tile.Type;
                }
            }

            // New objects
            foreach (Entity ent in p.NewObjs)
            {
                Lib_K_Relay.GameData.DataStructures.ObjectStructure objStruct = GameData.Objects.ByID(ent.ObjectType);

                // Objects with 0 maximum HP are immobile world elements
                // These objects are stored inside of the tiles on which they're standing
                if (objStruct.MaxHP == 0 && objStruct.ObjectClass != "Pet")
                {
                    // Get the tile coordinates
                    int x = (int)ent.Status.Position.X;
                    int y = (int)ent.Status.Position.Y;

                    // Create a reference to the tile
                    ref GameMapTile tile = ref Tiles[x, y];

                    // Create the tile if it's null
                    if (tile == null)
                    {
                        tile = new GameMapTile();
                    }

                    // Get the object type
                    ushort type = ent.ObjectType;

                    // Add the object type to the tile if the tile doesn't contain it yet
                    if (!tile.Objects.Contains(type))
                    {
                        Tiles[x, y].Objects.Add(type);
                    }
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