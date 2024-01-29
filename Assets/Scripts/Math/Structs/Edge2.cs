using UnityEngine;

namespace Retrover.Math
{
    public class Edge2
    {
        public Vector2 Point1 { get; private set; }
        public Vector2 Point2 { get; private set; }

        public bool IsIntersecting { get; set; } = false;

        public Edge2(Vector2 point1, Vector2 point2)
        {
            Point1 = point1;
            Point2 = point2;
        }

        public override bool Equals(object obj)
        {
            return obj is Edge2 e && e.Point1.Equals(Point1) && e.Point2.Equals(Point2);
        }

        public override int GetHashCode()
        {
            return Point1.GetHashCode() ^ Point2.GetHashCode();
        }
    }
}
