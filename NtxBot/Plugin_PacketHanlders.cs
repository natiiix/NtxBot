using System;
using System.Linq;
using System.Collections.Generic;
using Lib_K_Relay.Networking;
using Lib_K_Relay.Networking.Packets;
using Lib_K_Relay.Networking.Packets.Server;
using Lib_K_Relay.Networking.Packets.DataObjects;
using Lib_K_Relay.GameData;
using Lib_K_Relay.GameData.DataStructures;

namespace NtxBot
{
    public partial class Plugin
    {
        private string mapName;
        private List<Tile> tiles;

        private Location lastKnownPlayerLocation;

        private void OnUpdate(Client client, Packet p)
        {
            UpdatePacket up = (UpdatePacket)p;
            if (up == null) return;

            // Create the tile list if it doesn't exist
            if (tiles == null)
            {
                tiles = new List<Tile>();
            }

            // Add the new tiles to the list
            tiles.AddRange(up.Tiles);

            // Get the player's current location
            Location playerLocation = client.PlayerData.Pos;

            // Null current player locaiton
            if (playerLocation == null)
            {
                Log("Player location = null");
            }
            // Last known player location not set
            else if (lastKnownPlayerLocation == null)
            {
                lastKnownPlayerLocation = playerLocation;
            }
            // Player has moved
            else if (playerLocation.X != lastKnownPlayerLocation.X || playerLocation.Y != lastKnownPlayerLocation.Y)
            {
                Log("Moved from " + lastKnownPlayerLocation.ToString() + " to " + playerLocation.ToString());

                lastKnownPlayerLocation = playerLocation;

                // Find what tile is the player standing on
                int currentTileIdx = tiles.FindIndex(x => x.X == (short)playerLocation.X && x.Y == (short)playerLocation.Y);

                if (currentTileIdx >= 0)
                {
                    Log(GameData.Tiles.ByID(tiles[currentTileIdx].Type).Name);
                }
            }

            // TODO
        }

        private void OnMapInfo(Client client, Packet p)
        {
            // Convert the generic packet to a specific one
            MapInfoPacket mip = p as MapInfoPacket;
            if (mip == null) return;

            // Copy the name of the map
            mapName = mip.Name;

            // Clear the map tiles list
            tiles?.Clear();

            // Write the current map to the log
            Log("Current map: " + mapName ?? "null");
        }

        private void OnNewTick(Client client, Packet p)
        {
            NewTickPacket ntp = (NewTickPacket)p;
            if (ntp == null) return;

            // TODO
        }

        private void OnText(Client client, Packet p)
        {
            // Convert the packet
            TextPacket tp = p as TextPacket;
            if (tp == null) return;

            // TODO
        }
    }
}