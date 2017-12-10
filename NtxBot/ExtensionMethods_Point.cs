using Lib_K_Relay.Networking.Packets.DataObjects;
using System.Drawing;

namespace NtxBot
{
    public static partial class ExtensionMethods
    {
        public static Location ToLocation(Point p) => new Location(p.X, p.Y);
    }
}