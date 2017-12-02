using Lib_K_Relay;
using Lib_K_Relay.Interface;
using Lib_K_Relay.Networking.Packets;
using Lib_K_Relay.Utilities;

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

            proxy.HookPacket(PacketType.UPDATE, OnUpdate);
            proxy.HookPacket(PacketType.NEWTICK, OnNewTick);
            proxy.HookPacket(PacketType.MAPINFO, OnMapInfo);
            proxy.HookPacket(PacketType.TEXT, OnText);

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