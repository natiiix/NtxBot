using Lib_K_Relay.Networking;

namespace NtxBot
{
    public class AbyssBot
    {
        private Client client;
        private FlashClient flash;
        private GameMap map;

        public AbyssBot(Client client, FlashClient flash, GameMap map)
        {
            this.client = client;
            this.flash = flash;
            this.map = map;
        }

        public void Run()
        {
            // TODO
        }
    }
}