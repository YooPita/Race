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
    }
}
