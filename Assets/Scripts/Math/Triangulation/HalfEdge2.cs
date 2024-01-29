using System;
using System.Collections.Generic;

namespace Retrover.Math
{
    public class HalfEdge2
    {
        public HalfEdgeVertex2 Vertex { get; set; }
        public HalfEdgeFace2 Face { get; set; }
        public HalfEdge2 NextEdge { get; set; }
        public HalfEdge2 OppositeEdge { get; set; }
        public HalfEdge2 PreviousEdge { get; set; }

        public HalfEdge2(HalfEdgeVertex2 vertex)
        {
            Vertex = vertex ?? throw new ArgumentNullException(nameof(vertex));
        }

        public override string ToString()
        {
            return $"({Face})";
        }

        public void CalculateOppositeEdge()
        {
            HalfEdgeVertex2 endVertex = NextEdge.Vertex;
            HalfEdge2 current = endVertex.Edge;
            do
            {
                if (current.Vertex.Position.Equals(Vertex.Position))
                {
                    OppositeEdge = current;
                    break;
                }

                current = current.NextEdge;
            } while (current != endVertex.Edge);
        }

        public List<HalfEdge2> Flip()
        {
            // Ensure there is an opposite edge for flipping
            if (OppositeEdge == null)
            {
                throw new InvalidOperationException("Cannot flip an edge without an opposite edge.");
            }

            // Retrieve adjacent edges
            HalfEdge2 a = this;
            HalfEdge2 b = a.NextEdge;
            HalfEdge2 c = b.NextEdge;
            HalfEdge2 d = OppositeEdge;
            HalfEdge2 e = d.NextEdge;
            HalfEdge2 f = e.NextEdge;

            // Acquire vertices
            HalfEdgeVertex2 p1 = b.Vertex;
            HalfEdgeVertex2 p2 = d.Vertex;
            HalfEdgeVertex2 p3 = a.Vertex;
            HalfEdgeVertex2 p4 = c.Vertex;

            // Obtain faces
            HalfEdgeFace2 face1 = a.Face;
            HalfEdgeFace2 face2 = d.Face;

            // Reorient the edges
            a.Vertex = p4;
            b.Vertex = p2;
            d.Vertex = p1;
            e.Vertex = p3;

            // Update edge connections
            a.NextEdge = e;
            a.PreviousEdge = f;

            d.NextEdge = b;
            d.PreviousEdge = c;

            b.NextEdge = c;
            b.PreviousEdge = d;

            e.NextEdge = f;
            e.PreviousEdge = a;

            c.NextEdge = d;
            c.PreviousEdge = b;

            f.NextEdge = a;
            f.PreviousEdge = e;

            // Update faces
            face1.Edge = a;
            face2.Edge = d;

            a.Face = face1;
            b.Face = face2;
            c.Face = face2;

            d.Face = face1;
            e.Face = face1;
            f.Face = face2;

            return new List<HalfEdge2> { this, NextEdge, PreviousEdge, OppositeEdge, OppositeEdge.NextEdge, OppositeEdge.PreviousEdge };
        }
    }
}
