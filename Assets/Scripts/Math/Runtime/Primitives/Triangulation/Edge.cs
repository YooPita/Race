using UnityEngine;

namespace Retrover.Math
{
    public struct Edge
    {
        public Vector2 Start { get; private set; }
        public Vector2 End { get; private set; }

        public Edge(Vector2 start, Vector2 end)
        {
            Start = start;
            End = end;
        }

        public override readonly bool Equals(object obj)
        {
            if (obj is Edge other)
            {
                return (Start.Equals(other.Start) && End.Equals(other.End)) ||
                       (Start.Equals(other.End) && End.Equals(other.Start));
            }

            return false;
        }

        public override readonly int GetHashCode()
        {
            int startHashCode = Start.GetHashCode();
            int endHashCode = End.GetHashCode();

            return startHashCode ^ endHashCode;
        }

        public override readonly string ToString()
        {
            return $"start: {Start}, end: {End}";
        }

        public readonly bool Intersects(Vector2 p1, Vector2 p2)
        {
            return LinesIntersect(Start, End, p1, p2);
        }

        public readonly Vector2 ProjectedPoint(Vector2 point)
        {
            Vector2 edgeVector = End - Start;
            Vector2 pointVector = point - Start;

            float dotProduct = Vector2.Dot(pointVector, edgeVector);

            float edgeLengthSquared = edgeVector.sqrMagnitude;

            float normalizedDistance = dotProduct / edgeLengthSquared;

            normalizedDistance = Mathf.Clamp(normalizedDistance, 0, 1);

            return Start + normalizedDistance * edgeVector;
        }

        private static bool LinesIntersect(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
        {
            Vector2 s1 = p1 - p0;
            Vector2 s2 = p3 - p2;

            float s, t;
            s = (-s1.y * (p0.x - p2.x) + s1.x * (p0.y - p2.y)) / (-s2.x * s1.y + s1.x * s2.y);
            t = (s2.x * (p0.y - p2.y) - s2.y * (p0.x - p2.x)) / (-s2.x * s1.y + s1.x * s2.y);

            return (s >= 0 && s <= 1 && t >= 0 && t <= 1);
        }
    }
}
