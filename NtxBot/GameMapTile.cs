using System;
using System.Collections.Generic;
using Lib_K_Relay.GameData;

namespace NtxBot
{
    public class GameMapTile : IPathNode<System.Object>
    {
        private static readonly ushort TILE_PARTIAL_RED_FLOOR = GameData.Objects.ByName("Partial Red Floor").ID;

        public ushort? TileType;
        public List<ushort> Objects = new List<ushort>();

        public bool Safe
        {
            get => (TileType != null && GameData.Tiles.ByID(TileType.Value).MaxDamage == 0) || Objects.Contains(TILE_PARTIAL_RED_FLOOR);
        }

        public GameMapTile()
        {
            TileType = null;
        }

        public GameMapTile(ushort type)
        {
            TileType = type;
        }

        public bool IsWalkable(object inContext) => Safe;
    }
}