/*
using Lib_K_Relay;
using Lib_K_Relay.Utilities;
using Lib_K_Relay.Interface;
using Lib_K_Relay.Networking;
using Lib_K_Relay.Networking.Packets;
using Lib_K_Relay.Networking.Packets.Client;
using Lib_K_Relay.Networking.Packets.Server;
using Lib_K_Relay.Networking.Packets.DataObjects;
*/

using Lib_K_Relay;
using Lib_K_Relay.Interface;
using Lib_K_Relay.Networking.Packets.DataObjects;
using Lib_K_Relay.Networking.Packets.Server;
using System.Threading.Tasks;

namespace NtxBot
{
    public partial class Plugin : IPlugin
    {
        private FormUI ui;

        public string GetAuthor() => "natiiix";

        public string[] GetCommands() => new string[0];

        public string GetDescription() => "RotMG bot by natiiix";

        public string GetName() => "NTX Bot";

        public void Initialize(Proxy proxy)
        {
            ShowUI();

            // Information about a specified tile
            proxy.HookCommand("tileinfo", (client, cmd, args) =>
            {
                int x = 0;
                int y = 0;

                if (args.Length == 0)
                {
                    x = (int)client.PlayerData.Pos.X;
                    y = (int)client.PlayerData.Pos.Y;
                }
                else if (args.Length == 2 && (!int.TryParse(args[0], out x) || !int.TryParse(args[1], out y)))
                {
                    Log("Invalid coordinates!");
                    return;
                }
                else if (args.Length != 2)
                {
                    Log("Unexpected number of arguments!");
                    return;
                }

                Log(map.Tiles[x, y].ToString());
            });

            // Information about player's location
            proxy.HookCommand("where", (client, cmd, args) =>
            {
                Log("Player's location: " + client.PlayerData.Pos.ToString());
            });

            // Information about living entities on the current map
            proxy.HookCommand("living", (client, cmd, args) =>
            {
                map.LivingEntities.ForEach(x => Log(ConvertObjectTypeToString(x.ObjectType)));
            });

            proxy.HookCommand("goto", (client, cmd, args) =>
            {
                new MovementEngine(client, new FlashClient(), map).BeginMove(new Point(int.Parse(args[0]), int.Parse(args[1])));
            });

            proxy.HookPacket<UpdatePacket>(OnUpdate);
            proxy.HookPacket<NewTickPacket>(OnNewTick);
            proxy.HookPacket<MapInfoPacket>(OnMapInfo);

            Log("Packets hooked");
        }

        private void Proxy_ClientConnected(Lib_K_Relay.Networking.Client client)
        {
            throw new System.NotImplementedException();
        }

        private void Log(string text) => ui?.AppendLog(text);

        private void ShowUI()
        {
            if (ui == null)
            {
                ui = new FormUI();
                ui.Show();
                //PluginUtils.ShowGUI(ui);
            }
        }
    }
}