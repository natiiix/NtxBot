﻿using Lib_K_Relay.GameData;
using Lib_K_Relay.GameData.DataStructures;
using Lib_K_Relay.Networking.Packets.DataObjects;
using Lib_K_Relay.Networking.Packets.Server;
using System.Collections.Generic;
using System.Linq;

namespace NtxBot
{
    public class GameMap
    {
        public readonly string Name;
        public readonly int Width;
        public readonly int Height;

        public GameMapTile[,] Tiles { get; private set; }
        public List<Entity> LivingEntities { get; private set; }

        public Entity QuestObject
        {
            get
            {
                if (questObjectId < 0)
                {
                    return null;
                }

                int idx = LivingEntities.FindIndex(x => x.Status.ObjectId == questObjectId);

                if (idx < 0)
                {
                    return null;
                }

                return LivingEntities[idx];
            }
        }

        private int questObjectId;

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
            questObjectId = -1;
        }

        public void ProcessPacket(UpdatePacket p)
        {
            // Tiles
            p.Tiles.ForEach(x => Tiles[x.X, x.Y].SetTile(x.Type));

            // New objects
            foreach (Entity ent in p.NewObjs)
            {
                // Get object structure
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
                    // Quest object
                    if (ent.Status.ObjectId == questObjectId)
                    //if (objStruct.Quest)
                    {
                        Plugin.Log("Quest object appeared: " + objStruct.Name);
                    }

                    LivingEntities.Add(ent);
                }
            }

            // Droped (no longer present) objects
            foreach (int objId in p.Drops)
            {
                if (objId == questObjectId)
                //if (LivingEntities.Exists(x => x.Status.ObjectId == objId) && GameData.Objects.ByID(LivingEntities.Find(x => x.Status.ObjectId == objId).ObjectType).Quest)
                {
                    Plugin.Log("Quest object dropped!");
                }

                // Remove objects with the specified object ID from the list
                LivingEntities.RemoveAll(x => x.Status.ObjectId == objId /*&& !GameData.Objects.ByID(x.ObjectType).Quest*/);
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

        public void ProcessPacket(QuestObjIdPacket p)
        {
            // New quest
            if (p.ObjectId != questObjectId)
            {
                questObjectId = p.ObjectId;

                // This shouldn't happen
                if (QuestObject != null)
                {
                    Plugin.Log("Quest object is already present!");
                }
            }
        }

        public IEnumerable<Point> FindShortestPath(Point playerLocation, Point target)
        {
            LinkedList<GameMapTile> shortestPath = new SpatialAStar<GameMapTile>(Tiles).Search(playerLocation, target);
            return shortestPath?.Select(x => x.Location);
        }
    }
}