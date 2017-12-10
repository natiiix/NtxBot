using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing;
using Lib_K_Relay.GameData;
using Lib_K_Relay.Networking;
using Lib_K_Relay.Networking.Packets;
using Lib_K_Relay.Networking.Packets.DataObjects;
using Lib_K_Relay.Networking.Packets.Server;
using Lib_K_Relay.Networking.Packets.Client;
using Lib_K_Relay.Utilities;

namespace NtxBot
{
    public class AbyssBot
    {
        private Client client;
        private GameMap map;
        private FlashClient flash;

        public AbyssBot(Client client, GameMap map, FlashClient flash)
        {
            this.client = client;
            this.map = map;
            this.flash = flash;
        }

        public void Run()
        {
            // TODO
        }

        public void MoveSafely(Location target)
        {
            MovementEngine me = new MovementEngine(client, flash);

            FindShortestSafePath(target).ForEach(x =>
            {
                me.BeginMove(x);

                do
                {
                    Thread.Sleep(100);
                }
                while (me.Moving);

                PluginUtils.Log("Abyss Bot", "Movement stopped!");
            });

            PluginUtils.Log("Abyss Bot", "Done moving!");
        }

        private IEnumerable<Location> FindShortestSafePath(Location target)
        {
            return new SpatialAStar<GameMapTile, object>(map.Tiles).Search(client.PlayerData.Pos.ToPoint(), target.ToPoint(), null).Select(x => new Location(x.X, x.Y));
        }
    }
}