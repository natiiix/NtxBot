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

                // An object has to fall under one of these categories to be considered a living entity
                if (objStruct.Player || objStruct.Enemy || objStruct.God || objStruct.Pet || objStruct.Quest)
                {
                    // Add new living entities to the list
                    LivingEntities.Add(ent);

                    // Quest object
                    if (ent.Status.ObjectId == questObjectId)
                    {
                        Plugin.Log("Current quest: " + objStruct.Name);
                    }
                }
                // Everything else is considered an static world element
                // Static world elements are stored inside of the tiles on which they're standing
                else
                {
                    // Get the tile coordinates
                    int x = (int)ent.Status.Position.X;
                    int y = (int)ent.Status.Position.Y;

                    // Add the object to the tile
                    Tiles[x, y].AddObject(objStruct);
                }
            }

            // Droped (no longer present) objects
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

        public void ProcessPacket(QuestObjIdPacket p)
        {
            // New quest
            if (p.ObjectId != questObjectId)
            {
                // Copy the object ID
                questObjectId = p.ObjectId;
            }
        }

        public IEnumerable<Point> FindShortestPath(Point playerLocation, Point target)
        {
            LinkedList<GameMapTile> shortestPath = new SpatialAStar<GameMapTile>(Tiles).Search(playerLocation, target);
            return shortestPath?.Select(x => x.Location);
        }
    }
}