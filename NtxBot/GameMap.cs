using Lib_K_Relay.GameData;
using Lib_K_Relay.GameData.DataStructures;
using Lib_K_Relay.Networking;
using Lib_K_Relay.Networking.Packets;
using Lib_K_Relay.Networking.Packets.DataObjects;
using Lib_K_Relay.Networking.Packets.Server;
using Lib_K_Relay.Networking.Packets.Client;
using System;
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

        // Quest
        private int questObjectId;

        public Entity QuestObject { get => FindEntityById(questObjectId); }

        // Player
        private int playerObjectId;

        private Entity PlayerObject { get => FindEntityById(playerObjectId); }

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
            playerObjectId = -1;
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
                //LivingEntities.FindAll(x => x.Status.ObjectId == stat.ObjectId).ForEach(x => x.Status = stat);

                LivingEntities.FindAll(x => x.Status.ObjectId == stat.ObjectId).ForEach(x =>
                {
                    // Update object's position
                    x.Status.Position = stat.Position;

                    // Update status data
                    foreach (StatData data in stat.Data)
                    {
                        for (int i = 0; i < x.Status.Data.Length; i++)
                        {
                            if (data.Id == x.Status.Data[i].Id)
                            {
                                x.Status.Data[i] = data;
                            }
                        }
                    }
                });
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

        /* Visualization of the square radius meaning
         * 22222
         * 21112
         * 21012
         * 21112
         * 22222
         */

        public List<Point> GetWalkableTilesInSquareRadius(Point baseTile, int radius)
        {
            if (radius < 0)
            {
                throw new ArgumentException("Radius must not be negative.");
            }

            // Get the boundaries of the square
            int left = baseTile.X - radius;
            int right = baseTile.X + radius;
            int top = baseTile.Y - radius;
            int bottom = baseTile.Y + radius;

            List<Point> tiles = new List<Point>();

            // Vertical sides
            for (int y = (top >= 0 ? top : 0); y <= (bottom < Height ? bottom : Height - 1); y++)
            {
                // Left side tile
                if (left >= 0 && Tiles[left, y].Walkable)
                {
                    tiles.Add(new Point(left, y));
                }

                // Right side tile
                if (right < Width && Tiles[right, y].Walkable)
                {
                    tiles.Add(new Point(right, y));
                }
            }

            // Horizontal side
            for (int x = (left >= 0 ? left : 0); x < (right < Width ? right : Width - 1); x++)
            {
                // Top side tile
                if (top >= 0 && Tiles[x, top].Walkable)
                {
                    tiles.Add(new Point(x, top));
                }

                // Bottom side tile
                if (bottom < Height && Tiles[x, bottom].Walkable)
                {
                    tiles.Add(new Point(x, bottom));
                }
            }

            return tiles;
        }

        public IEnumerable<Point> GetWalkableTilesInDistanceFromTile(Point baseTile, double minDistance, double maxDistance)
        {
            // Maximum distance must not be lower than minumum distance
            if (maxDistance < minDistance)
            {
                throw new ArgumentException("Maximum distance must be higher than minimum distance.");
            }

            // Get value of the lowest and the highest radius to consider when searching for walkable tiles
            int minRadius = (int)Math.Floor(minDistance);
            int maxRadius = (int)Math.Ceiling(maxDistance);

            List<Point> allTiles = new List<Point>();

            // Get tiles from every radius within the limits
            for (int radius = minRadius; radius <= maxRadius; radius++)
            {
                allTiles.AddRange(GetWalkableTilesInSquareRadius(baseTile, radius));
            }

            // Returns tiles within the specified distance
            return allTiles.Where(x =>
            {
                double dist = x.DistanceTo(baseTile);
                return dist >= minDistance && dist <= maxDistance;
            });
        }

        public bool UsePlayerAbility(Client client, Location usePosition = null)
        {
            Entity player = PlayerObject;

            // Client object doesn't exist
            if (player == null)
            {
                return false;
            }

            // Iterate through player data
            foreach (StatData stat in player.Status.Data)
            {
                // If player has an ability item equipped
                if (stat.Id == StatsType.Inventory1 && stat.IntValue >= 0)
                {
                    UseItemPacket p = Packet.Create<UseItemPacket>(PacketType.USEITEM);

                    p.Time = client.Time;
                    p.ItemUsePos = usePosition ?? client.PlayerData.Pos;
                    p.UseType = 1;

                    p.SlotObject = new SlotObject()
                    {
                        ObjectId = client.ObjectId,
                        ObjectType = stat.IntValue,
                        SlotId = 1
                    };

                    client.SendToServer(p);
                    return true;
                }
            }

            // Unable to find an equipped ability item
            return false;
        }

        public void SetPlayerObjectId(int objectId) => playerObjectId = objectId;

        private Entity FindEntityById(int objectId)
        {
            if (objectId < 0)
            {
                return null;
            }

            int idx = LivingEntities.FindIndex(x => x.Status.ObjectId == objectId);

            if (idx < 0)
            {
                return null;
            }

            return LivingEntities[idx];
        }
    }
}