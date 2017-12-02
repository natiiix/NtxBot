using System;
using System.Linq;
using System.Collections.Generic;
using Lib_K_Relay.Networking;
using Lib_K_Relay.Networking.Packets;
using Lib_K_Relay.Networking.Packets.Server;
using Lib_K_Relay.Networking.Packets.DataObjects;

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

            // Get the player's current location
            Location playerLocation = client.PlayerData.Pos;

            if (playerLocation == null)
            {
                Log("Player location = null");
            }
            else if (lastKnownPlayerLocation == null)
            {
                lastKnownPlayerLocation = playerLocation;
            }
            else if (playerLocation.X != lastKnownPlayerLocation.X || playerLocation.Y != lastKnownPlayerLocation.Y)
            {
                Log("Moved from " + lastKnownPlayerLocation.ToString() + " to " + playerLocation.ToString());

                lastKnownPlayerLocation = playerLocation;
            }

            // Create the tile list if it doesn't exist
            if (tiles == null)
            {
                tiles = new List<Tile>();
            }

            // Add the new tiles to the list
            tiles.AddRange(up.Tiles);

            int currentTileIdx = tiles.FindIndex(x => x.X == ((int)Math.Round(playerLocation.X)) && x.Y == ((int)Math.Round(playerLocation.Y)));

            if (currentTileIdx >= 0)
            {
                Log(tiles[currentTileIdx].Type.ToString());
            }

            //{
            //    Log("Number of new tiles: " + up.Tiles.Length.ToString());

            //    Tile firstTile = up.Tiles.First();
            //    Log(string.Format("Tile: X={0} Y={1} Type={2}", firstTile.X, firstTile.Y, firstTile.Type));

            //    Log(string.Format("Player: X={0} Y={1}", (short)Math.Round(playerLocation.X), (short)Math.Round(playerLocation.Y)));
            //}

            // Find the tile on which the player is currently standing
            //List<Tile> standingOn = currentMapTiles.Where(x => x.X == (short)Math.Round(playerLocation.X) && x.X == (short)Math.Round(playerLocation.Y)).ToList();

            //if (standingOn.Count > 0)
            //{
            //    Log(string.Format("Standing on {0} tiles: {1}", standingOn.Count, string.Join(", ", standingOn.Select(x => x.Type.ToString()))));
            //}

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