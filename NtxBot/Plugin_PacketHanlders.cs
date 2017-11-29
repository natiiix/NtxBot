using Lib_K_Relay.Networking;
using Lib_K_Relay.Networking.Packets;
using Lib_K_Relay.Networking.Packets.Server;

namespace NtxBot
{
    public partial class Plugin
    {
        private void OnUpdate(Client client, Packet p)
        {
            UpdatePacket up = (UpdatePacket)p;
            if (up == null) return;

            // TODO
        }

        private void OnMapInfo(Client client, Packet p)
        {
            // Convert the generic packet to a specific one
            MapInfoPacket mip = p as MapInfoPacket;
            if (mip == null) return;

            // Copy the name of the map
            currentMapName = mip.Name;

            // Write the current map to the log
            Log("Current map: " + currentMapName);
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