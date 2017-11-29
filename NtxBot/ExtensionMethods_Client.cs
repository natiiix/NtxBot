using Lib_K_Relay.Networking;
using Lib_K_Relay.Networking.Packets;
using Lib_K_Relay.Networking.Packets.Client;

namespace NtxBot
{
    public static partial class ExtensionMethods
    {
        private static void TeleportToNexus(this Client client)
        {
            // Send an escape packet to the server
            client.SendToServer(Packet.Create(PacketType.ESCAPE));
        }

        private static void SendChatMessage(this Client client, string text)
        {
            // Create the packet
            PlayerTextPacket ptp = (PlayerTextPacket)Packet.Create(PacketType.PLAYERTEXT);
            // Set the text of the message
            ptp.Text = text;
            // Send the packet to the server
            client.SendToServer(ptp);
        }
    }
}