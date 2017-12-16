using Lib_K_Relay.Networking;
using Lib_K_Relay.Networking.Packets.Client;
using Lib_K_Relay.Networking.Packets.Server;

namespace NtxBot
{
    public partial class Plugin
    {
        private const double AUTOHEAL_THRESHOLD = 0.75;

        public static bool blockNextGotoAck = false;

        private GameMap map;

        private void OnUpdate(Client client, UpdatePacket p)
        {
            map?.ProcessPacket(p);
        }

        private void OnNewTick(Client client, NewTickPacket p)
        {
            map?.ProcessPacket(p);

            // Heal the player if health is below the threshold
            if (flash != null && (double)client.PlayerData.Health / client.PlayerData.MaxHealth < AUTOHEAL_THRESHOLD)
            {
                flash.UseAbility();
                Log("Auto-heal triggered!");
            }
        }

        private void OnMapInfo(Client client, MapInfoPacket p)
        {
            // Create a new map object
            map = new GameMap(p);

            // Write the name of the map to the log
            Log("Current map: " + map.Name ?? "null");

            // Give the player a reminder to bind the Flash Player
            if (flash == null)
            {
                Log("Use the /flash command to enable auto-heal!");
            }
        }

        private void OnGotoAck(Client client, GotoAckPacket p)
        {
            if (blockNextGotoAck)
            {
                p.Send = false;
                blockNextGotoAck = false;
            }
        }

        private void OnQuestObjId(Client client, QuestObjIdPacket p)
        {
            map?.ProcessPacket(p);
        }
    }
}