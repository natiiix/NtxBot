using Lib_K_Relay.Networking;
using Lib_K_Relay.Networking.Packets;
using Lib_K_Relay.Networking.Packets.Client;
using Lib_K_Relay.Utilities;
using Lib_K_Relay.Networking.Packets.Server;
using Lib_K_Relay.Networking.Packets.DataObjects;
using System.Threading.Tasks;

namespace NtxBot
{
    public static partial class ExtensionMethods
    {
        public static void TeleportToNexus(this Client client)
        {
            // Send an escape packet to the server
            client.SendToServer(Packet.Create<EscapePacket>(PacketType.ESCAPE));
        }

        public static void SendChatMessage(this Client client, string text)
        {
            // Create the packet
            PlayerTextPacket ptp = Packet.Create<PlayerTextPacket>(PacketType.PLAYERTEXT);
            // Set the text of the message
            ptp.Text = text;
            // Send the packet to the server
            client.SendToServer(ptp);
        }

        public static void DisplayChatNotification(this Client client, string text, string name = "NTX Bot")
        {
            // Send an Oryx notification to the client with the specified message
            client.SendToClient(PluginUtils.CreateOryxNotification(name, text));
        }

        public static void DisplayInGameNotification(this Client client, string text)
        {
            // Send a notification to the client
            // The notification will be displayed on top of the character's head
            client.SendToClient(PluginUtils.CreateNotification(client.ObjectId, text));
        }

        public static Point GetPlayerLocationAsPoint(this Client client) => (Point)client.PlayerData.Pos;

        //public static void MoveUsingGoTo(this Client client, Location target)
        //{
        //    const int delay = 200; // ms

        //    while (client.Connected)
        //    {
        //        Location playerPos = client.PlayerData.Pos;
        //        double distanceToTarget = playerPos.DistanceSquaredTo(target);

        //        if (distanceToTarget <= 0)
        //        {
        //            return;
        //        }

        //        GotoPacket gp = Packet.Create<GotoPacket>(PacketType.GOTO);
        //        gp.ObjectId = client.ObjectId;

        //        double maxDistance = client.PlayerData.TilesPerTick() / 1000.0 * delay;

        //        if (distanceToTarget < maxDistance)
        //        {
        //            gp.Location = target;
        //        }
        //        else
        //        {
        //            double multiplier = maxDistance / distanceToTarget;
        //            Location partialTarget = new Location(
        //                (float)(playerPos.X + ((target.X - playerPos.X) * multiplier)),
        //                (float)(playerPos.Y + ((target.Y - playerPos.Y) * multiplier)));

        //            gp.Location = partialTarget;
        //        }

        //        Plugin.blockNextGotoAck = true;
        //        client.SendToClient(gp);
        //        Task.Delay(delay);
        //    }
        //}
    }
}