using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Retrover.Math
{
    public class TriangulationData
    {
        public HashSet<HalfEdgeVertex2> Vertices { get; private set; } = new HashSet<HalfEdgeVertex2>();
        public HashSet<HalfEdgeFace2> Faces { get; private set; } = new HashSet<HalfEdgeFace2>();
        public HashSet<HalfEdge2> Edges { get; private set; } = new HashSet<HalfEdge2>();
        public HashSet<Vector2> Points { get; private set; }
        public Triangle2 SuperTriangle { get; private set; } = new(new Vector2(-100f, -100f), new Vector2(100f, -100f), new Vector2(0f, 100f));

        public TriangulationData(HashSet<Vector2> points)
        {
            Points = points ?? throw new ArgumentNullException(nameof(points));
        }

        public void AddFace(Vector2 point1, Vector2 point2, Vector2 point3)
        {
            // Creating vertices
            HalfEdgeVertex2 vertex1 = GetOrCreateVertex(point1);
            HalfEdgeVertex2 vertex2 = GetOrCreateVertex(point2);
            HalfEdgeVertex2 vertex3 = GetOrCreateVertex(point3);

            // Creating edges
            HalfEdge2 edge1 = new(vertex1);
            HalfEdge2 edge2 = new(vertex2);
            HalfEdge2 edge3 = new(vertex3);

            // Setting connections between edges
            vertex1.Edge = edge1;
            vertex2.Edge = edge2;
            vertex3.Edge = edge3;

            edge1.NextEdge = edge2;
            edge1.PreviousEdge = edge3;
            edge2.NextEdge = edge3;
            edge2.PreviousEdge = edge1;
            edge3.NextEdge = edge1;
            edge3.PreviousEdge = edge2;

            // Creating a face
            HalfEdgeFace2 face = new(edge1);

            // Setting connections between edges and face
            edge1.Face = face;
            edge2.Face = face;
            edge3.Face = face;

            // Adding edges and face to the sets
            Edges.Add(edge1);
            Edges.Add(edge2);
            Edges.Add(edge3);
            Faces.Add(face);

            Vertices.Add(vertex1);
            Vertices.Add(vertex2);
            Vertices.Add(vertex3);

            edge1.CalculateOppositeEdge();
            edge2.CalculateOppositeEdge();
            edge3.CalculateOppositeEdge();
        }

        private HalfEdgeVertex2 GetOrCreateVertex(Vector2 point)
        {
            var existingVertex = Vertices.FirstOrDefault(v => v.Position.Equals(point));
            if (existingVertex != null)
            {
                return existingVertex;
            }

            var newVertex = new HalfEdgeVertex2(point);
            Vertices.Add(newVertex);
            return newVertex;
        }

        public void RemoveFace(Vector2 point1, Vector2 point2, Vector2 point3)
        {
            var pointsToFind = new List<Vector2> { point1, point2, point3 };

            foreach (var face in Faces)
            {
                var faceVertices = new HashSet<Vector2>(
                    face.GetVertices().Select(v => v.Position));

                // Checking if the vertices correspond to the required points
                if (faceVertices.SetEquals(pointsToFind))
                {
                    RemoveFace(face);
                    break;
                }
            }
        }

        private void RemoveFace(HalfEdgeFace2 faceToRemove)
        {
            // Removing related edges
            Edges.Remove(faceToRemove.Edge);
            Edges.Remove(faceToRemove.Edge.NextEdge);
            Edges.Remove(faceToRemove.Edge.PreviousEdge);

            // Checking and removing vertices if they are no longer used
            RemoveVertexIfUnused(faceToRemove.Edge.Vertex);
            RemoveVertexIfUnused(faceToRemove.Edge.NextEdge.Vertex);
            RemoveVertexIfUnused(faceToRemove.Edge.PreviousEdge.Vertex);

            // Removing the face
            Faces.Remove(faceToRemove);
        }

        private void RemoveVertexIfUnused(HalfEdgeVertex2 vertex)
        {
            if (!Edges.Any(e => e.Vertex == vertex))
            {
                Vertices.Remove(vertex);
            }
        }

        public IEnumerable<HalfEdge2> GetEdgesAroundPoint(Vector2 point)
        {
            return Edges.Where(edge => edge.Vertex.Position.Equals(point));
        }

        public bool IsEdgeOnStack(HalfEdge2 edge, Stack<HalfEdge2> edgeStack)
        {
            return edgeStack.Contains(edge);
        }
    }
}
