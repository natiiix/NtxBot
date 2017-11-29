using Lib_K_Relay.Networking;
using Lib_K_Relay.Networking.Packets;
using Lib_K_Relay.Networking.Packets.Server;

namespace NtxBot
{
    public partial class Plugin
    {
        private void OnUpdate(Client client, Packet p)
        {
        }

        private void OnMapInfo(Client client, Packet p)
        {
            // Convert the generic packet to a specific one
            MapInfoPacket mip = p as MapInfoPacket;

            // Ignore if null
            if (mip == null)
            {
                return;
            }

            // Copy the name of the map
            currentMapName = mip.Name;

            Log("Current map: " + currentMapName);
        }

        private void OnHit(Client client, Packet p)
        {
        }

        private void OnNewTick(Client client, Packet p)
        {
        }

        private void OnText(Client client, Packet p)
        {
        }
    }
}