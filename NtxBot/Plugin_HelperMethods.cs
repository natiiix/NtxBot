using Lib_K_Relay.Networking;

namespace NtxBot
{
    public partial class Plugin
    {
        public static void Log(string text) => ui?.AppendLog(text);

        private static void ShowUI()
        {
            if (ui == null)
            {
                ui = new FormUI();
            }

            ui.Show();
            //PluginUtils.ShowGUI(ui);
        }
    }
}