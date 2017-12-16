using Lib_K_Relay.Networking;
using Lib_K_Relay.Networking.Packets.Server;
using Lib_K_Relay.Networking.Packets.Client;
using Lib_K_Relay.Networking.Packets.DataObjects;
using System.Linq;

namespace NtxBot
{
    public partial class Plugin
    {
        private const double AUTOHEAL_THRESHOLD = 0.75;

        public static bool blockNextGotoAck = false;

        private GameMap map;

        //private List<ushort> distinctTiles = new List<ushort>();
        //private List<ushort> distinctObjects = new List<ushort>();

        private void OnUpdate(Client client, UpdatePacket p)
        {
            map?.ProcessPacket(p);

            // Add distinct tiles and object types to the lists
            //distinctTiles.AddRange(up.Tiles.Where(x => !distinctTiles.Contains(x.Type)).Select(x => x.Type));
            //distinctObjects.AddRange(up.NewObjs.Where(x => !distinctObjects.Contains(x.ObjectType)).Select(x => x.ObjectType));

            //up.NewObjs.Where(x => GameData.Objects.ByID(x.ObjectType).MaxHP == 0 && GameData.Objects.ByID(x.ObjectType).Size != 0).ForEach(x =>
            //{
            //    Lib_K_Relay.GameData.DataStructures.ObjectStructure obj = GameData.Objects.ByID(x.ObjectType);
            //    Log(ConvertObjectTypeToString(x.ObjectType) + string.Format(" [ {0} | {1} | {2} | {3} | {4} | {5} | {6} | {7} | {8} | {9} | {10} ]", obj.ObjectClass, obj.Static, obj.OccupySquare, obj.FullOccupy, obj.Flying, obj.DrawOnGround, obj.Defense, obj.Size, obj.ShadowSize, obj.XPMult, obj.MaxHP));
            //});
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
            // Distinct tile and objects are map-specific
            //distinctTiles?.Clear();
            //distinctObjects?.Clear();

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