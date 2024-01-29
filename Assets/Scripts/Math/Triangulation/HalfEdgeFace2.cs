using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Retrover.Math
{
    public class HalfEdgeFace2
    {
        public HalfEdge2 Edge { get; set; }

        public HalfEdgeFace2(HalfEdge2 edge)
        {
            Edge = edge ?? throw new ArgumentNullException(nameof(edge));
        }

        public override string ToString()
        {
            var vertices = GetVertices().ToList();
            return $"({vertices[0].Position}, {vertices[1].Position}, {vertices[2].Position})";
        }

        public IEnumerable<HalfEdgeVertex2> GetVertices()
        {
            yield return Edge.Vertex;
            yield return Edge.NextEdge.Vertex;
            yield return Edge.PreviousEdge.Vertex;
        }

        public IEnumerable<HalfEdge2> SplitFace(Vector2 splitPosition)
        {
            // Creating a new vertex at the split position
            HalfEdgeVertex2 newVertex = new(splitPosition);

            // Retrieving the original edges of the triangle
            HalfEdge2 originalEdge1 = Edge;
            HalfEdge2 originalEdge2 = Edge.NextEdge;
            HalfEdge2 originalEdge3 = Edge.PreviousEdge;

            // Creating new edges
            HalfEdge2 newEdge1 = new(newVertex);
            HalfEdge2 newEdge2 = new(newVertex);
            HalfEdge2 newEdge3 = new(newVertex);

            // Setting up new connections between edges
            newEdge1.NextEdge = originalEdge1.NextEdge;
            newEdge1.PreviousEdge = newEdge2;

            newEdge2.NextEdge = originalEdge2.NextEdge;
            newEdge2.PreviousEdge = newEdge3;

            newEdge3.NextEdge = originalEdge3.NextEdge;
            newEdge3.PreviousEdge = newEdge1;

            originalEdge1.NextEdge.PreviousEdge = newEdge1;
            originalEdge2.NextEdge.PreviousEdge = newEdge2;
            originalEdge3.NextEdge.PreviousEdge = newEdge3;

            originalEdge1.NextEdge = newEdge1;
            originalEdge2.NextEdge = newEdge2;
            originalEdge3.NextEdge = newEdge3;

            // Creating new faces
            HalfEdgeFace2 newFace1 = new(newEdge1);
            HalfEdgeFace2 newFace2 = new(newEdge2);
            HalfEdgeFace2 newFace3 = new(newEdge3);

            // Establishing connections between edges and faces
            newEdge1.Face = newFace1;
            newEdge2.Face = newFace2;
            newEdge3.Face = newFace3;

            originalEdge1.Face = newFace1;
            originalEdge2.Face = newFace2;
            originalEdge3.Face = newFace3;

            // Returning new edges
            return new List<HalfEdge2> { newEdge1, newEdge2, newEdge3 };
        }
    }
}
