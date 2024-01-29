using UnityEngine;

namespace Retrover.Math
{
    public struct Triangle2
    {
        public Vector2 Point1 { get; private set; }
        public Vector2 Point2 { get; private set; }
        public Vector2 Point3 { get; private set; }

        public Triangle2(Vector2 point1, Vector2 point2, Vector2 point3)
        {
            Point1 = point1;
            Point2 = point2;
            Point3 = point3;
        }

        public Triangle2 OrientClockwise()
        {
            if (!IsTriangleOrientedClockwise())
                return new Triangle2(Point2, Point1, Point3);

            return this;
        }

        public readonly float MinX() => Mathf.Min(Point1.x, Mathf.Min(Point2.x, Point3.x));
        public readonly float MaxX() => Mathf.Max(Point1.x, Mathf.Max(Point2.x, Point3.x));
        public readonly float MinY() => Mathf.Min(Point1.y, Mathf.Min(Point2.y, Point3.y));
        public readonly float MaxY() => Mathf.Max(Point1.y, Mathf.Max(Point2.y, Point3.y));

        public readonly Edge2 FindOppositeEdgeToVertex(Vector2 point)
        {
            if (point == Point1) return new Edge2(Point2, Point3);
            if (point == Point2) return new Edge2(Point3, Point1);
            return new Edge2(Point1, Point2);
        }

        public readonly bool IsEdgePartOfTriangle(Edge2 edge)
        {
            return edge == new Edge2(Point1, Point2) || edge == new Edge2(Point2, Point3) || edge == new Edge2(Point3, Point1);
        }

        public readonly Vector2 GetVertexWhichIsNotPartOfEdge(Edge2 edge)
        {
            if (!Point1.Equals(edge.Point1) && !Point1.Equals(edge.Point2)) return Point1;
            if (!Point2.Equals(edge.Point1) && !Point2.Equals(edge.Point2)) return Point2;
            return Point3;
        }

        private bool IsTriangleOrientedClockwise()
        {
            float determinant = Point1.x * Point2.y + Point3.x * Point1.y + Point2.x * Point3.y
                              - Point1.x * Point3.y - Point3.x * Point2.y - Point2.x * Point1.y;

            return determinant > 0f;
        }
    }
}
