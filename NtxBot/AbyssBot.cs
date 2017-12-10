using Lib_K_Relay.GameData;
using Lib_K_Relay.Networking;
using Lib_K_Relay.Networking.Packets.DataObjects;
using System.Collections.Generic;
using System.Linq;

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
                me.Move(x);
            });
        }

        private IEnumerable<Location> FindShortestSafePath(Location target)
        {
            return new SpatialAStar<GameMapTile, object>(map.Tiles).Search(client.PlayerData.Pos.ToPoint(), target.ToPoint(), null)
                // Construct a location from the tile coordinates
                // Make it point to the center of the tile rather than the top-left corner
                .Select(x => new Location(x.X + 0.5f, x.Y + 0.5f));
        }
    }
}