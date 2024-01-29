using UnityEngine;

namespace Retrover.Math
{
    public class HalfEdgeVertex2
    {
        public Vector2 Position { get; private set; }
        public HalfEdge2 Edge { get; set; }

        public HalfEdgeVertex2(Vector2 position)
        {
            Position = position;
        }

        public override string ToString()
        {
            return $"({Position})";
        }
    }
}
