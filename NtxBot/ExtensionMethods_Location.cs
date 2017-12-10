using Lib_K_Relay.Networking.Packets.DataObjects;
using System.Drawing;

namespace NtxBot
{
    public static partial class ExtensionMethods
    {
        public static Point ToPoint(this Location loc) => new Point((int)loc.X, (int)loc.Y);
    }
}