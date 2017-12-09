using System;
using System.Collections.Generic;
using Lib_K_Relay.GameData;

namespace NtxBot
{
    public struct GameMapTile : IPathNode<Object>
    {
        private static readonly ushort TILE_PARTIAL_RED_FLOOR = GameData.Objects.ByName("Partial Red Floor").ID;

        public ushort? TileType;
        public List<ushort> Objects;

        public bool Safe
        {
            get => (TileType != null && GameData.Tiles.ByID(TileType.Value).MaxDamage == 0) || Objects.Contains(TILE_PARTIAL_RED_FLOOR);
        }

        public GameMapTile(ushort? type = null)
        {
            TileType = type;
            Objects = new List<ushort>();
        }

        public bool IsWalkable(object inContext) => Safe;
    }
}