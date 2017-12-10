using Lib_K_Relay.GameData;
using Lib_K_Relay.GameData.DataStructures;
using System.Collections.Generic;
using System.Linq;

namespace NtxBot
{
    public class GameMapTile : IPathNode
    {
        public Point Location { get; private set; }

        public bool Walkable { get; private set; }

        private TileStructure? tileStruct = null;
        private List<ObjectStructure> objStructs = new List<ObjectStructure>();

        public GameMapTile(Point location)
        {
            Location = location;
            Walkable = false;
        }

        public GameMapTile(int x, int y) : this(new Point(x, y))
        {
        }

        public void SetTile(ushort tileType)
        {
            if (tileStruct == null || tileStruct.Value.ID != tileType)
            {
                tileStruct = GameData.Tiles.ByID(tileType);
                EvaluateWalkableProperty();
            }
        }

        public void AddObject(ObjectStructure objStruct)
        {
            if (!objStructs.Contains(objStruct))
            {
                objStructs.Add(objStruct);
                EvaluateWalkableProperty();
            }
        }

        private void EvaluateWalkableProperty()
        {
            // Null tiles are implicitly not walkable
            if (tileStruct == null || !tileStruct.HasValue)
            {
                Walkable = false;
                return;
            }

            // Tiles with NoWalk property and objects with OccupySquare property are not walkable
            if (tileStruct.Value.NoWalk || objStructs.Exists(x => x.OccupySquare))
            {
                Walkable = false;
                return;
            }

            // Tiles that can cause damage to the player are only walkable if there is an object
            // that protects the player from taking ground damage on top of them
            if (tileStruct.Value.MaxDamage > 0)
            {
                Walkable = objStructs.Exists(x => x.ProtectFromGroundDamage);
                return;
            }

            Walkable = true;
        }

        public override string ToString()
        {
            return string.Format("X={0} Y={1} Walkable={2} Type={3} Objects={4}",
                Location.X, Location.Y, Walkable,
                tileStruct.HasValue ? ("{" + tileStruct.Value.ID.ToString() + " - " + tileStruct.Value.Name + "}") : "NULL",
                "[" + string.Join(", ", objStructs.Select(x => "{" + x.ID + " - " + x.Name + "}")) + "]");
        }
    }
}