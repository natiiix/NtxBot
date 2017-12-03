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
    public static partial class ExtensionMethods
    {
        public static bool HasHP(this Entity ent) => ent.Status.Data.ToList().FindIndex(stat => stat.Id == StatsType.HP) >= 0;
    }
}