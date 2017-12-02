using Lib_K_Relay.GameData;
using Lib_K_Relay.Networking;
using Lib_K_Relay.Networking.Packets;
using Lib_K_Relay.Networking.Packets.DataObjects;
using Lib_K_Relay.Networking.Packets.Server;
using System.Collections.Generic;
using System.Linq;

namespace NtxBot
{
    public partial class Plugin
    {
        private string mapName;
        private List<Tile> tiles = new List<Tile>();
        private List<Entity> objects = new List<Entity>();

        private Location lastKnownPlayerLocation;

        private void OnUpdate(Client client, Packet p)
        {
            UpdatePacket up = (UpdatePacket)p;
            if (up == null) return;

            // Add the new tiles and objects to their respective lists
            tiles.AddRange(up.Tiles);
            objects.AddRange(up.NewObjs);

            // Remove all the dropped (no longer present) objects
            up.Drops.ForEach(x => objects.RemoveAll(y => y.Status.ObjectId == x));

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
                //Log("Moved from " + lastKnownPlayerLocation.ToString() + " to " + playerLocation.ToString());
                lastKnownPlayerLocation = playerLocation;

                // Find what tile is the player standing on
                int currentTileIdx = tiles.FindIndex(x => x.X == (short)playerLocation.X && x.Y == (short)playerLocation.Y);

                if (currentTileIdx >= 0)
                {
                    Log(playerLocation.ToString() + " : " + tiles[currentTileIdx].Type.ToString() + " - " + GameData.Tiles.ByID(tiles[currentTileIdx].Type).Name);
                }

                // Find all objects located on the tile the player is currently standing on and write information about them to the log
                objects.Where(x => (int)x.Status.Position.X == (int)playerLocation.X && (int)x.Status.Position.Y == (int)playerLocation.Y).ForEach(x =>
                {
                    Log(x.ObjectType.ToString() + " - " + GameData.Objects.ByID(x.ObjectType).Name);
                });
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

            // Clear the map-specific lists
            tiles?.Clear();
            objects?.Clear();

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