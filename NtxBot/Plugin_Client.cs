using Lib_K_Relay.Networking;
using Lib_K_Relay.Networking.Packets;

namespace NtxBot
{
    public partial class Plugin
    {
        private static void TeleportToNexus(Client client)
        {
            client.SendToServer(Packet.Create(PacketType.ESCAPE));
        }
    }
}