using UnityEngine;

namespace Retrover.Math
{
    public static class GeometryUtils
    {
        private const float Epsilon = 0.001f;

        public static bool IsPointInsideTriangle(Vector2 a, Vector2 b, Vector2 c, Vector2 p)
        {
            float area = TriangleArea(a, b, c);
            float area1 = TriangleArea(p, b, c);
            float area2 = TriangleArea(a, p, c);
            float area3 = TriangleArea(a, b, p);

            return Mathf.Abs(area - (area1 + area2 + area3)) < Epsilon;
        }

        private static float TriangleArea(Vector2 a, Vector2 b, Vector2 c)
        {
            return Mathf.Abs((a.x * (b.y - c.y) + b.x * (c.y - a.y) + c.x * (a.y - b.y)) / 2.0f);
        }

        public static bool ShouldFlipEdge(HalfEdge2 edge)
        {
            if (edge.OppositeEdge == null)
                return false;

            Vector2 p1 = edge.Vertex.Position;
            Vector2 p2 = edge.NextEdge.Vertex.Position;
            Vector2 p3 = edge.PreviousEdge.Vertex.Position;
            Vector2 pOpposite = edge.OppositeEdge.NextEdge.Vertex.Position;

            return IsPointInsideCircumcircle(p1, p2, p3, pOpposite);
        }

        public static bool IsPointToTheRightOrOnLine(Vector2 a, Vector2 b, Vector2 p)
        {
            return GetPointInRelationToVectorValue(a, b, p) <= Epsilon;
        }

        private static float GetPointInRelationToVectorValue(Vector2 a, Vector2 b, Vector2 p)
        {
            float deltaX1 = a.x - p.x;
            float deltaY1 = a.y - p.y;
            float deltaX2 = b.x - p.x;
            float deltaY2 = b.y - p.y;

            return deltaX1 * deltaY2 - deltaX2 * deltaY1;
        }

        private static bool IsPointInsideCircumcircle(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p)
        {
            float[,] matrix = {
            { p1.x - p.x, p1.y - p.y, (p1.x - p.x) * (p1.x - p.x) + (p1.y - p.y) * (p1.y - p.y) },
            { p2.x - p.x, p2.y - p.y, (p2.x - p.x) * (p2.x - p.x) + (p2.y - p.y) * (p2.y - p.y) },
            { p3.x - p.x, p3.y - p.y, (p3.x - p.x) * (p3.x - p.x) + (p3.y - p.y) * (p3.y - p.y) }
        };

            return Determinant(matrix) > 0;
        }

        private static float Determinant(float[,] matrix)
        {
            return matrix[0, 0] * (matrix[1, 1] * matrix[2, 2] - matrix[1, 2] * matrix[2, 1]) -
                   matrix[0, 1] * (matrix[1, 0] * matrix[2, 2] - matrix[1, 2] * matrix[2, 0]) +
                   matrix[0, 2] * (matrix[1, 0] * matrix[2, 1] - matrix[1, 1] * matrix[2, 0]);
        }
    }
}
