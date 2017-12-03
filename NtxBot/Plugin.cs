using Lib_K_Relay;
using Lib_K_Relay.Interface;
using Lib_K_Relay.Networking.Packets;
using Lib_K_Relay.Networking.Packets.DataObjects;
using Lib_K_Relay.Utilities;
using System.Linq;
using System;

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

            //proxy.HookCommand("distinct", (client, cmd, args) =>
            //{
            //    Log("---- TILES ----");
            //    distinctTiles.ForEach(x => Log(ConvertTileTypeToString(x)));

            //    Log("---- OBJECTS ----");
            //    distinctObjects.ForEach(x => Log(ConvertObjectTypeToString(x)));
            //});

            proxy.HookCommand("tileinfo", (client, cmd, args) =>
            {
                // Find the tile
                int x = (int)client.PlayerData.Pos.X;
                int y = (int)client.PlayerData.Pos.Y;

                GameMapTile tile = map.GetTile(x, y);

                if (tile == null)
                {
                    Log("Null tile!");
                }
                else
                {
                    // Tile type
                    Log("Tile type: " + ConvertTileTypeToString(tile.TileType));

                    // List of objects on that tile
                    tile.Objects.ForEach(obj => Log(ConvertObjectTypeToString(obj)));
                }
            });

            proxy.HookPacket(PacketType.UPDATE, OnUpdate);
            proxy.HookPacket(PacketType.NEWTICK, OnNewTick);
            proxy.HookPacket(PacketType.MAPINFO, OnMapInfo);
            proxy.HookPacket(PacketType.GOTOACK, OnGotoAck);

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