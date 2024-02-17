using System.Collections.Generic;
using UnityEngine;

namespace Retrover.Math
{
    public class DelaunayTriangulation
    {
        public List<Triangle> Triangles { get; private set; } = new();

        private IPointsGroup _points;
        private Triangle _superTriangle;

        public DelaunayTriangulation(IPointsGroup points)
        {
            _points = points;
        }

        public void Calculate()
        {
            _superTriangle = _points.SuperTriangle();
            Triangles.Add(new(_superTriangle.Vertex1, _superTriangle.Vertex2, _superTriangle.Vertex3));

            foreach (Vector2 point in _points)
            {
                List<Triangle> badTriangles = FindTrianglesContainingPoint(point);
                CreateNewTriangles(point, badTriangles);
            }
        }

        public void RemoveTrianglesContainingSuperTriangleVertices()
        {
            Vector2 vertex1 = _superTriangle.Vertex1;
            Vector2 vertex2 = _superTriangle.Vertex2;
            Vector2 vertex3 = _superTriangle.Vertex3;

            Triangles.RemoveAll(triangle =>
                triangle.Vertex1.Equals(vertex1) || triangle.Vertex2.Equals(vertex1) || triangle.Vertex3.Equals(vertex1) ||
                triangle.Vertex1.Equals(vertex2) || triangle.Vertex2.Equals(vertex2) || triangle.Vertex3.Equals(vertex2) ||
                triangle.Vertex1.Equals(vertex3) || triangle.Vertex2.Equals(vertex3) || triangle.Vertex3.Equals(vertex3)
            );
        }

        private List<Triangle> FindTrianglesContainingPoint(Vector2 point)
        {
            List<Triangle> containingTriangles = new();

            foreach (Triangle triangle in Triangles)
                if (triangle.ContainsInCircumcircle(point))
                    containingTriangles.Add(triangle);

            return containingTriangles;
        }

        private void CreateNewTriangles(Vector2 point, List<Triangle> badTriangles)
        {
            List<Edge> polygonEdges = new();

            foreach (Triangle triangle in badTriangles)
            {
                polygonEdges.Add(triangle.Edge1);
                polygonEdges.Add(triangle.Edge2);
                polygonEdges.Add(triangle.Edge3);
            }

            for (int i = polygonEdges.Count - 1; i >= 0; i--)
            {
                for (int j = i - 1; j >= 0; j--)
                {
                    if (polygonEdges[i].Equals(polygonEdges[j]))
                    {
                        polygonEdges.RemoveAt(i);
                        polygonEdges.RemoveAt(j);
                        i--;
                        break;
                    }
                }
            }

            foreach (Edge edge in polygonEdges)
                Triangles.Add(new(edge.Start, edge.End, point));

            foreach (Triangle badTriangle in badTriangles)
                Triangles.Remove(badTriangle);
        }
    }
}
