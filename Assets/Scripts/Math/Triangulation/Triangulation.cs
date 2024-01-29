using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Retrover.Math
{
    public class Triangulation
    {
        public HashSet<HalfEdgeVertex2> Vertices { get; private set; } = new();

        public HashSet<HalfEdgeFace2> Faces { get; private set; } = new();
        public HashSet<HalfEdge2> Edges { get; private set; } = new();

        private const float Epsilon = 0.00001f;

        private readonly Triangle2 SuperTriangle = new(new Vector2(-100f, -100f), new Vector2(100f, -100f), new Vector2(0f, 100f));

        private readonly HashSet<Vector2> _points;

        public Triangulation(HashSet<Vector2> points)
        {
            _points = points;
        }

        public void Calculate()
        {
            //Step 2. Sort the points into bins to make it faster to find which triangle a point is in
            //TODO

            //Step 3. Establish the supertriangle
            AddTriangle(new HashSet<Triangle2>() { SuperTriangle });

            //Step 5-7
            foreach (Vector2 point in _points)
                InsertNewPointInTriangulation(point);

            //Step 8. Delete the vertices belonging to the supertriangle
            RemoveSuperTriangle();
        }

        public void AddTriangle(HashSet<Triangle2> triangles)
        {
            triangles = triangles.Select(x => x.OrientClockwise()).ToHashSet();

            Dictionary<(Vector2, Vector2), HalfEdge2> edgeLookup = new();

            foreach (Triangle2 triangle in triangles)
            {
                HalfEdgeVertex2 vertex1 = new(triangle.Point1);
                HalfEdgeVertex2 vertex2 = new(triangle.Point2);
                HalfEdgeVertex2 vertex3 = new(triangle.Point3);

                //The vertices the edge points to
                HalfEdge2 halfEdge1 = new(vertex1);
                HalfEdge2 halfEdge2 = new(vertex2);
                HalfEdge2 halfEdge3 = new(vertex3);

                halfEdge1.NextEdge = halfEdge2;
                halfEdge2.NextEdge = halfEdge3;
                halfEdge3.NextEdge = halfEdge1;

                halfEdge1.PreviousEdge = halfEdge3;
                halfEdge2.PreviousEdge = halfEdge1;
                halfEdge3.PreviousEdge = halfEdge2;

                //The vertex needs to know of an edge going from it
                vertex1.Edge = halfEdge2;
                vertex2.Edge = halfEdge3;
                vertex3.Edge = halfEdge1;

                //The face the half-edge is connected to
                HalfEdgeFace2 face = new(halfEdge1);

                //Each edge needs to know of the face connected to this edge
                halfEdge1.Face = face;
                halfEdge2.Face = face;
                halfEdge3.Face = face;

                //Add everything to the lists
                Edges.Add(halfEdge1);
                Edges.Add(halfEdge2);
                Edges.Add(halfEdge3);

                Faces.Add(face);

                Vertices.Add(vertex1);
                Vertices.Add(vertex2);
                Vertices.Add(vertex3);
            }

            foreach (HalfEdge2 edge in Edges)
            {
                HalfEdgeVertex2 goingToVertex = edge.Vertex;
                HalfEdgeVertex2 goingFromVertex = edge.PreviousEdge.Vertex;

                foreach (HalfEdge2 otherEdge in Edges)
                {
                    //Dont compare with itself
                    if (edge == otherEdge)
                        continue;

                    //Is this edge going between the vertices in the opposite direction
                    if (goingFromVertex.Position.Equals(otherEdge.Vertex.Position)
                        && goingToVertex.Position.Equals(otherEdge.PreviousEdge.Vertex.Position))
                    {
                        edge.OppositeEdge = otherEdge;

                        break;
                    }
                }
            }
        }

        public HashSet<HalfEdge2> GetUniqueEdges()
        {
            var uniqueEdges = new HashSet<HalfEdge2>();
            var edgeDictionary = new Dictionary<(Vector2, Vector2), HalfEdge2>();

            foreach (HalfEdge2 edge in Edges)
            {
                var p1 = edge.Vertex.Position;
                var p2 = edge.PreviousEdge.Vertex.Position;

                var edgeKey = (p1, p2);
                var reverseEdgeKey = (p2, p1);

                if (!edgeDictionary.ContainsKey(edgeKey) && !edgeDictionary.ContainsKey(reverseEdgeKey))
                {
                    edgeDictionary[edgeKey] = edge;
                    uniqueEdges.Add(edge);
                }
            }

            return uniqueEdges;
        }

        private void RemoveSuperTriangle()
        {
            var verticesOfSuperTriangle = new HashSet<Vector2> { SuperTriangle.Point1, SuperTriangle.Point2, SuperTriangle.Point3 };
            var facesToDelete = new HashSet<HalfEdgeFace2>();

            foreach (var vertex in Vertices)
            {
                if (!verticesOfSuperTriangle.Contains(vertex.Position) || facesToDelete.Contains(vertex.Edge.Face))
                    continue;

                facesToDelete.Add(vertex.Edge.Face);
            }

            foreach (var face in facesToDelete)
                DeleteTriangleFace(face, true);
        }

        private void InsertNewPointInTriangulation(Vector2 point)
        {
            //Step 5. Insert the new point in the triangulation
            //Find the existing triangle the point is in
            HalfEdgeFace2 face = AddPoint(point);

            //Delete this triangle and form 3 new triangles by connecting p to each of the vertices in the old triangle
            SplitTriangleFaceAtPoint(face, point);


            //Step 6. Initialize stack. Place all triangles which are adjacent to the edges opposite p on a LIFO stack
            //The report says we should place triangles, but it's easier to place edges with our data structure
            Stack<HalfEdge2> trianglesToInvestigate = new();

            AddTrianglesOppositePToStack(point, trianglesToInvestigate);


            //Step 7. Restore delaunay triangulation
            //While the stack is not empty
            int safety = 0;

            while (trianglesToInvestigate.Count > 0)
            {
                safety += 1;

                if (safety > 1000000)
                {
                    Debug.Log("Stuck in infinite loop when restoring delaunay in incremental sloan algorithm");

                    break;
                }

                //Step 7.1. Remove a triangle from the stack
                HalfEdge2 edgeToTest = trianglesToInvestigate.Pop();

                //Step 7.2. Do we need to flip this edge? 
                //If p is outside or on the circumcircle for this triangle, we have a delaunay triangle and can return to next loop
                Vector2 a = edgeToTest.Vertex.Position;
                Vector2 b = edgeToTest.PreviousEdge.Vertex.Position;
                Vector2 c = edgeToTest.NextEdge.Vertex.Position;

                //abc are here counter-clockwise
                if (ShouldFlipEdgeStable(a, b, c, point))
                {
                    FlipTriangleEdge(edgeToTest);

                    //Step 7.3. Place any triangles which are now opposite p on the stack
                    AddTrianglesOppositePToStack(point, trianglesToInvestigate);
                }
            }
        }

        private HalfEdgeFace2 AddPoint(Vector2 point)
        {
            HalfEdgeFace2 currentTriangle = GetRandomTriangle();

            HalfEdgeFace2 intersectingTriangle = null;
            int safetyCounter = 0;

            while (safetyCounter < 1000000)
            {
                safetyCounter++;

                HalfEdge2[] edges = { currentTriangle.Edge, currentTriangle.Edge.NextEdge, currentTriangle.Edge.NextEdge.NextEdge };
                bool isInside = true;

                foreach (var edge in edges)
                {
                    if (!IsPointToTheRightOrOnLine(edge.PreviousEdge.Vertex.Position, edge.Vertex.Position, point))
                    {
                        currentTriangle = edge.OppositeEdge.Face;
                        isInside = false;
                        break;
                    }
                }

                if (isInside)
                {
                    intersectingTriangle = currentTriangle;
                    break;
                }
            }

            if (safetyCounter >= 1000000)
                throw new System.Exception("Stuck in endless loop when walking in triangulation");


            return intersectingTriangle;
        }

        private HalfEdgeFace2 GetRandomTriangle()
        {
            int randomPos = Random.Range(0, Faces.Count);
            return Faces.ElementAt(randomPos);
        }

        private static bool IsPointToTheRightOrOnLine(Vector2 a, Vector2 b, Vector2 p)
        {
            float relationValue = GetPointInRelationToVectorValue(a, b, p);

            return relationValue <= Epsilon;
        }

        private static float GetPointInRelationToVectorValue(Vector2 vectorA, Vector2 vectorB, Vector2 point)
        {
            float deltaX1 = vectorA.x - point.x;
            float deltaY1 = vectorA.y - point.y;
            float deltaX2 = vectorB.x - point.x;
            float deltaY2 = vectorB.y - point.y;

            return deltaX1 * deltaY2 - deltaX2 * deltaY1;
        }

        private void SplitTriangleFaceAtPoint(HalfEdgeFace2 face, Vector2 splitPosition)
        {
            var edges = new List<HalfEdge2> { face.Edge, face.Edge.NextEdge, face.Edge.NextEdge.NextEdge };
            var newEdges = edges.Select(e => CreateNewFace(e, splitPosition)).ToHashSet();
            FindOppositeEdges(ref newEdges);
            DeleteTriangleFace(face);//, false);
        }

        private HalfEdge2 CreateNewFace(HalfEdge2 edge, Vector2 splitPosition)
        {
            HalfEdgeVertex2 newVertex = new HalfEdgeVertex2(splitPosition);

            HalfEdge2 newEdge = new(newVertex)
            {
                PreviousEdge = edge,
                NextEdge = edge.NextEdge
            };

            edge.NextEdge = newEdge;

            Vertices.Add(newVertex);
            Edges.Add(newEdge);

            newVertex.Edge = newEdge;

            return newEdge;
        }

        private static void FindOppositeEdges(ref HashSet<HalfEdge2> newEdges)
        {
            foreach (var e in newEdges)
            {
                if (e.OppositeEdge != null) continue;

                var eGoingTo = e.Vertex.Position;
                var eGoingFrom = e.PreviousEdge.Vertex.Position;

                foreach (var eOpposite in newEdges)
                {
                    if (e == eOpposite || eOpposite.OppositeEdge != null) continue;

                    var eGoingTo_Other = eOpposite.Vertex.Position;
                    var eGoingFrom_Other = eOpposite.PreviousEdge.Vertex.Position;

                    if (eGoingTo.Equals(eGoingFrom_Other) && eGoingFrom.Equals(eGoingTo_Other))
                    {
                        e.OppositeEdge = eOpposite;
                        eOpposite.OppositeEdge = e;
                    }
                }
            }
        }

        private void DeleteTriangleFace(HalfEdgeFace2 face, bool deleteIsolatedVertices)
        {
            HalfEdge2 edge1 = face.Edge;
            HalfEdge2 edge2 = edge1.NextEdge;
            HalfEdge2 edge3 = edge2.NextEdge;

            Edges.Remove(edge1);
            Edges.Remove(edge2);
            Edges.Remove(edge3);

            Faces.Remove(face);

            if (deleteIsolatedVertices)
            {
                DeleteVertexIfIsolated(edge1.Vertex);
                DeleteVertexIfIsolated(edge2.Vertex);
                DeleteVertexIfIsolated(edge3.Vertex);
            }
        }

        private void DeleteVertexIfIsolated(HalfEdgeVertex2 vertex)
        {
            if (vertex.Edge == null || Edges.All(e => e.Vertex != vertex))
            {
                Vertices.Remove(vertex);
            }
        }

        private void AddTrianglesOppositePToStack(Vector2 p, Stack<HalfEdge2> trianglesOppositeP)
        {
            HalfEdgeVertex2 rotateAroundThis = Vertices.FirstOrDefault(v => v.Position.Equals(p));

            if (rotateAroundThis == null)
                return;

            HalfEdgeFace2 startFace = rotateAroundThis.Edge.Face;
            HalfEdgeFace2 currentFace = null;

            do
            {
                HalfEdge2 edgeOppositeRotateVertex = rotateAroundThis.Edge.NextEdge.OppositeEdge;

                if (edgeOppositeRotateVertex != null && !trianglesOppositeP.Contains(edgeOppositeRotateVertex))
                    trianglesOppositeP.Push(edgeOppositeRotateVertex);

                rotateAroundThis = rotateAroundThis.Edge.OppositeEdge.Vertex;
                currentFace = rotateAroundThis.Edge.Face;
            }
            while (currentFace != startFace);
        }

        private static bool ShouldFlipEdgeStable(Vector2 v1, Vector2 v2, Vector2 v3, Vector2 vp)
        {
            Vector2 diff_13 = v1 - v3;
            Vector2 diff_23 = v2 - v3;
            Vector2 diff_1p = v1 - vp;
            Vector2 diff_2p = v2 - vp;

            float cos_a = VectorDotProduct(diff_13, diff_23);
            float cos_b = VectorDotProduct(diff_2p, diff_1p);

            if (cos_a >= 0f && cos_b >= 0f)
                return false;

            if (cos_a < 0f && cos_b < 0f)
                return true;

            float sin_ab = VectorCrossProduct(diff_13, diff_23) * cos_b + VectorCrossProduct(diff_2p, diff_1p) * cos_a;

            return sin_ab < 0;
        }

        private static float VectorDotProduct(Vector2 a, Vector2 b)
        {
            return a.x * b.x + a.y * b.y;
        }

        private static float VectorCrossProduct(Vector2 a, Vector2 b)
        {
            return a.x * b.y - a.y * b.x;
        }

        private static void FlipTriangleEdge(HalfEdge2 edge)
        {
            HalfEdge2 e1 = edge, e2 = edge.NextEdge, e3 = edge.PreviousEdge;
            HalfEdge2 e4 = edge.OppositeEdge, e5 = e4.NextEdge, e6 = e4.PreviousEdge;

            HalfEdgeVertex2 vA = e3.Vertex, vB = e2.Vertex, vC = e1.Vertex, vD = e5.Vertex;

            e1.Vertex = vB; e2.Vertex = vD; e3.Vertex = vC;
            e4.Vertex = vA; e5.Vertex = vD; e6.Vertex = vC;

            e1.NextEdge = e3; e1.PreviousEdge = e5;
            e2.NextEdge = e4; e2.PreviousEdge = e6;
            e3.NextEdge = e5; e3.PreviousEdge = e1;
            e4.NextEdge = e6; e4.PreviousEdge = e2;
            e5.NextEdge = e1; e5.PreviousEdge = e3;
            e6.NextEdge = e2; e6.PreviousEdge = e4;

            HalfEdgeFace2 f1 = e1.Face, f2 = e4.Face;

            e1.Face = f1; e3.Face = f1; e5.Face = f1;
            e2.Face = f2; e4.Face = f2; e6.Face = f2;

            vB.Edge = e3; vC.Edge = e5; vD.Edge = e1;
            vA.Edge = e4; vD.Edge = e6; vC.Edge = e2;

            f1.Edge = e3; f2.Edge = e4;
        }
    }

    public class HalfEdge2
    {
        public HalfEdgeVertex2 Vertex { get; set; }
        public HalfEdgeFace2 Face { get; set; }
        public HalfEdge2 NextEdge { get; set; }
        public HalfEdge2 OppositeEdge { get; set; }
        public HalfEdge2 PreviousEdge { get; set; }

        public HalfEdge2(HalfEdgeVertex2 vertex)
        {
            Vertex = vertex;
        }
    }

    public class HalfEdgeVertex2
    {
        public Vector2 Position { get; private set; }
        public HalfEdge2 Edge { get; set; }

        public HalfEdgeVertex2(Vector2 position)
        {
            Position = position;
        }
    }

    public class HalfEdgeFace2
    {
        public HalfEdge2 Edge { get; set; }

        public HalfEdgeFace2(HalfEdge2 edge)
        {
            Edge = edge;
        }
    }
}
