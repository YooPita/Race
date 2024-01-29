using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Retrover.Math
{
    public class TriangulationCalculator
    {
        private readonly TriangulationData _data;

        public TriangulationCalculator(TriangulationData data)
        {
            _data = data ?? throw new ArgumentNullException(nameof(data));
        }

        public void Calculate()
        {
            _data.AddFace(_data.SuperTriangle.Point1, _data.SuperTriangle.Point2, _data.SuperTriangle.Point3);
            TriangulatePoints();
            _data.RemoveFace(_data.SuperTriangle.Point1, _data.SuperTriangle.Point2, _data.SuperTriangle.Point3);
        }

        private void TriangulatePoints()
        {
            foreach (Vector2 point in _data.Points)
                InsertNewPointInTriangulation(point);
        }

        private void InsertNewPointInTriangulation(Vector2 point)
        {
            HalfEdgeFace2 face = FindTriangleContainingPoint(point);
            SplitTriangleFaceAtPoint(face, point);
            RestoreDelaunayTriangulation(point);
        }

        private HalfEdgeFace2 FindTriangleContainingPoint(Vector2 point)
        {
            foreach (var face in _data.Faces)
            {
                if (IsPointInsideTriangle(face, point))
                    return face;
            }
                

            throw new InvalidOperationException("No triangle found containing the point");
        }

        private bool IsPointInsideTriangle(HalfEdgeFace2 face, Vector2 point)
        {
            var vertices = face.GetVertices().ToList();
            return GeometryUtils.IsPointInsideTriangle(vertices[0].Position, vertices[1].Position, vertices[2].Position, point);
        }

        private void SplitTriangleFaceAtPoint(HalfEdgeFace2 face, Vector2 splitPosition)
        {
            var vertices = face.GetVertices().ToList();
            var v1 = vertices[0].Position;
            var v2 = vertices[1].Position;
            var v3 = vertices[2].Position;

            // Removing the original face
            _data.RemoveFace(v1, v2, v3);

            // Adding three new faces
            _data.AddFace(v1, v2, splitPosition);
            _data.AddFace(v2, v3, splitPosition);
            _data.AddFace(v3, v1, splitPosition);
        }

        private void RestoreDelaunayTriangulation(Vector2 point)
        {
            // Restoring Delaunay triangulation
            var edgeStack = new Stack<HalfEdge2>(_data.GetEdgesAroundPoint(point));

            while (edgeStack.Count > 0)
            {
                HalfEdge2 edge = edgeStack.Pop();
                if (GeometryUtils.ShouldFlipEdge(edge))
                {
                    var flippedEdges = edge.Flip();
                    foreach (var flippedEdge in flippedEdges)
                    {
                        if (!_data.IsEdgeOnStack(flippedEdge, edgeStack))
                        {
                            edgeStack.Push(flippedEdge);
                        }
                    }
                }
            }
        }
    }
}
