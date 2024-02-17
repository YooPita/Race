using UnityEngine;

namespace Retrover.Math
{
    public struct Circle
    {
        public Vector2 Center { get; private set; }
        public float Radius { get; private set; }

        public Circle(Vector2 center, float radius)
        {
            Center = center;
            Radius = radius;
        }

        public override readonly string ToString()
        {
            return $"center: {Center}, radius: {Radius}";
        }
    }
}
