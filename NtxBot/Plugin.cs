using Lib_K_Relay;
using Lib_K_Relay.Interface;
using Lib_K_Relay.Networking.Packets;
using Lib_K_Relay.Networking.Packets.Server;

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

            proxy.HookCommand("tileinfo", (client, cmd, args) =>
            {
                if (args.Length != 2)
                {
                    Log("Invalid arguments!");
                    return;
                }

                GameMapTile tile = map.Tiles[int.Parse(args[0]), int.Parse(args[1])];

                if (tile == null)
                {
                    Log("Null tile!");
                }
                else
                {
                    // Tile type
                    Log("Type: " + ConvertTileTypeToString(tile.TileType));

                    // List of objects on that tile
                    tile.Objects.ForEach(obj => Log(ConvertObjectTypeToString(obj)));

                    // Information about tile's safety
                    Log(tile.Safe ? "SAFE" : "UNSAFE");
                }
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

            proxy.HookPacket<UpdatePacket>(OnUpdate);
            proxy.HookPacket<NewTickPacket>(OnNewTick);
            proxy.HookPacket<MapInfoPacket>(OnMapInfo);

            Log("Packets hooked");
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