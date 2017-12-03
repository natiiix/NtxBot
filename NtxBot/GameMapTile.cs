using System.Collections.Generic;

namespace NtxBot
{
    public class GameMapTile
    {
        public ushort? TileType;
        public List<ushort> Objects = new List<ushort>();

        public GameMapTile()
        {
            TileType = null;
        }

        public GameMapTile(ushort type)
        {
            TileType = type;
        }
    }
}