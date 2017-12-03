﻿using System;
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

        private GameMapTile[] tiles;
        private List<Entity> livingEntities;

        public GameMap(MapInfoPacket mip)
        {
            Name = mip.Name;
            Width = mip.Width;
            Height = mip.Height;

            tiles = new GameMapTile[Height * Width];
            livingEntities = new List<Entity>();
        }

        private int CoordinatesToIndex(int x, int y) => (y * Width) + x;

        private bool AreValidCoordinates(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;

        public GameMapTile GetTile(int x, int y) => AreValidCoordinates(x, y) ? tiles[CoordinatesToIndex(x, y)] : null;

        public ushort? GetTileType(int x, int y)
        {
            GameMapTile tile = GetTile(x, y);

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
                // Get the file index
                int idx = CoordinatesToIndex(tile.X, tile.Y);

                // Create the tile if it doesn't exist
                if (tiles[idx] == null)
                {
                    tiles[idx] = new GameMapTile(tile.Type);
                }
                // Tile exist
                // Modify the tile type
                else
                {
                    tiles[idx].TileType = tile.Type;
                }
            }

            // New objects
            foreach (Entity ent in p.NewObjs)
            {
                // Objects with HP are not stored inside tile objects
                if (ent.HasHP())
                {
                    livingEntities.Add(ent);
                }
                else
                {
                    // Get the tile index
                    int x = (int)ent.Status.Position.X;
                    int y = (int)ent.Status.Position.Y;

                    int idx = CoordinatesToIndex(x, y);

                    // Create a reference to the tile
                    ref GameMapTile tile = ref tiles[idx];

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
                        tiles[idx].Objects.Add(type);
                    }
                }
            }

            // Drops
            foreach (int objId in p.Drops)
            {
                // Remove objects with the specified object ID from the list
                livingEntities.RemoveAll(x => x.Status.ObjectId == objId);
            }
        }

        public void ProcessPacket(NewTickPacket p)
        {
            // Update statuses of living entities
            foreach (Status stat in p.Statuses)
            {
                livingEntities.FindAll(x => x.Status.ObjectId == stat.ObjectId).ForEach(x => x.Status = stat);
            }
        }
    }
}