using Lib_K_Relay.Networking.Packets.DataObjects;

namespace NtxBot
{
    public class Point
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static explicit operator Point(Location a) => new Point((int)a.X, (int)a.Y);

        public static explicit operator Location(Point a) => new Location(a.X, a.Y);
    }
}