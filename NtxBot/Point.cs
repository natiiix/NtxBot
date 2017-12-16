using Lib_K_Relay.Networking.Packets.DataObjects;
using System;

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

        // Convert the point to a location pointing to the center of the tile rather than the top-left corner
        public static explicit operator Location(Point a) => new Location(a.X + 0.5f, a.Y + 0.5f);

        public override string ToString() => "{ X=" + X.ToString() + "; Y=" + Y.ToString() + " }";

        public static Point operator -(Point a, Point b) => new Point(a.X - b.X, a.Y - b.Y);

        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(Point))
            {
                Point p = obj as Point;
                return p.X == X && p.Y == Y;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode() => X.GetHashCode() ^ Y.GetHashCode();

        public double DistanceTo(Point p) => Math.Sqrt(Math.Pow(p.X - X, 2) + Math.Pow(p.Y - Y, 2));
    }
}