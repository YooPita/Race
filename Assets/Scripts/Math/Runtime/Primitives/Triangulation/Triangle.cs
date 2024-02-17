using System;
using UnityEngine;

namespace Retrover.Math
{
    public class Triangle
    {
        public Vector2 Vertex1 { get; private set; }
        public Vector2 Vertex2 { get; private set; }
        public Vector2 Vertex3 { get; private set; }

        public Edge Edge1 { get; private set; }
        public Edge Edge2 { get; private set; }
        public Edge Edge3 { get; private set; }

        public Circle Circumcircle => _circumcircle.Value;

        private Lazy<Circle> _circumcircle;
        private bool _isCircumcircleCalculated = false;

        const float Tolerance = 1e-5f;

        public Triangle(Vector2 vertex1, Vector2 vertex2, Vector2 vertex3)
        {
            Vertex1 = vertex1;
            Vertex2 = vertex2;
            Vertex3 = vertex3;

            Edge1 = new Edge(vertex1, vertex2);
            Edge2 = new Edge(vertex2, vertex3);
            Edge3 = new Edge(vertex3, vertex1);

            _circumcircle = new Lazy<Circle>(() => CalculateCircumcircle());
        }

        public bool ContainsInCircumcircle(Vector2 point)
        {
            if (!_isCircumcircleCalculated)
                CalculateCircumcircle();

            float distanceSquared = (point - Circumcircle.Center).sqrMagnitude;
            return distanceSquared < (Circumcircle.Radius * Circumcircle.Radius);
        }

        public override string ToString()
        {
            return $"({Vertex1}, {Vertex2}, {Vertex3})";
        }

        private Circle CalculateCircumcircle()
        {
            if (IsVerticesAlmostCollinear())
                return new Circle(Vertex1, float.Epsilon);

            float dA = Vertex1.x * Vertex1.x + Vertex1.y * Vertex1.y;
            float dB = Vertex2.x * Vertex2.x + Vertex2.y * Vertex2.y;
            float dC = Vertex3.x * Vertex3.x + Vertex3.y * Vertex3.y;

            float aux1 = (dA * (Vertex3.y - Vertex2.y) + dB * (Vertex1.y - Vertex3.y) + dC * (Vertex2.y - Vertex1.y));
            float aux2 = -(dA * (Vertex3.x - Vertex2.x) + dB * (Vertex1.x - Vertex3.x) + dC * (Vertex2.x - Vertex1.x));
            float div = (2 * (Vertex1.x * (Vertex3.y - Vertex2.y) + Vertex2.x * (Vertex1.y - Vertex3.y) + Vertex3.x * (Vertex2.y - Vertex1.y)));

            if (div == 0)
            {
                Vector2 center = new(float.MaxValue, float.MaxValue);
                float radius = float.MaxValue;
                return new Circle(center, radius);
            }
            else
            {
                Vector2 center = new(aux1 / div, aux2 / div);
                float radius = Vector2.Distance(center, Vertex1);
                return new Circle(center, radius);
            }
        }

        private bool IsVerticesAlmostCollinear()
        {
            Vector2 AB = Vertex2 - Vertex1;
            Vector2 AC = Vertex3 - Vertex1;

            float scaledTolerance = Tolerance * AB.magnitude * AC.magnitude;

            float crossProductLength = Mathf.Abs(AB.x * AC.y - AB.y * AC.x);
            return crossProductLength < scaledTolerance;
        }
    }
}
