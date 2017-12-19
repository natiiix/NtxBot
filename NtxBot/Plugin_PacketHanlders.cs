using Lib_K_Relay.Networking;
using Lib_K_Relay.Networking.Packets.Client;
using Lib_K_Relay.Networking.Packets.Server;
using Lib_K_Relay.Utilities;

namespace NtxBot
{
    public partial class Plugin
    {
        private const double AUTOHEAL_THRESHOLD = 0.75;

        public static bool blockNextGotoAck = false;

        private GameMap map;

        // Prevents auto-heal from being use on two consecutive ticks
        private bool autoHealReady = true;

        private void OnUpdate(Client client, UpdatePacket p)
        {
            map?.SetPlayerObjectId(client.ObjectId);
            map?.ProcessPacket(p);
        }

        private void OnNewTick(Client client, NewTickPacket p)
        {
            map?.ProcessPacket(p);

            // Heal the player if:
            if (
                // Auto-heal hasn't been triggered the last tick
                autoHealReady &&
                // playing priest
                client.PlayerData.Class == Classes.Priest &&
                // does not have Sick debuff (an attempt to heal with Sick would be a waste of mana)
                !client.PlayerData.HasConditionEffect(ConditionEffects.Sick) &&
                // health is below threshold
                (double)client.PlayerData.Health / client.PlayerData.MaxHealth < AUTOHEAL_THRESHOLD &&
                // has an ability item equipped
                map.PlayerAbility != null &&
                // has enough MP to use the ability
                client.PlayerData.Mana >= map.PlayerAbility.MpCost)
            {
                map?.UsePlayerAbility(client);
                autoHealReady = false;
                Log("Auto-heal triggered!");
            }
            else
            {
                autoHealReady = true;
            }
        }

        private void OnMapInfo(Client client, MapInfoPacket p)
        {
            // Create a new map object
            map = new GameMap(p);

            // Write the name of the map to the log
            Log("Current map: " + map.Name ?? "null");
        }

        private void OnGotoAck(Client client, GotoAckPacket p)
        {
            if (blockNextGotoAck)
            {
                p.Send = false;
                blockNextGotoAck = false;
            }
        }

        private void OnQuestObjId(Client client, QuestObjIdPacket p) => map?.ProcessPacket(p);
    }
}