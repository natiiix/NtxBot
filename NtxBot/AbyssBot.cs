using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib_K_Relay.GameData;
using Lib_K_Relay.Networking;
using Lib_K_Relay.Networking.Packets;
using Lib_K_Relay.Networking.Packets.DataObjects;
using Lib_K_Relay.Networking.Packets.Server;
using Lib_K_Relay.Networking.Packets.Client;

namespace NtxBot
{
    public class AbyssBot
    {
        private Client client;
        private GameMap map;

        public AbyssBot(Client client, GameMap map)
        {
            this.client = client;
            this.map = map;
        }

        public void Run()
        {
        }
    }
}