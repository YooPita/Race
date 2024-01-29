using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Retrover.Math
{
    public class Voronoi
    {
        public HashSet<VoronoiCell2> VoronoiCells { get; private set; } = new();

        private readonly PointArea _pointArea;
        private TriangulationData _triangulationData;

        public Voronoi(Rect area, HashSet<Vector2> points)
        {
            _pointArea = new PointArea(area, points);
        }

        public void Calculate()
        {
            Triangulation triangulation = new(_pointArea.Normalize().ToHashSet());
            triangulation.Calculate();
            _triangulationData = triangulation.Data;

            //Step 1. For every delaunay edge, compute a voronoi edge
            //The voronoi edge is the edge connecting the circumcenters of two neighboring delaunay triangles
            List<VoronoiEdge2> voronoiEdges = new List<VoronoiEdge2>();

            HashSet<HalfEdgeFace2> triangles = _triangulationData.Faces;

            //Loop through each triangle 
            foreach (HalfEdgeFace2 face in triangles)
            {
                //Each triangle consists of these edges
                HalfEdge2 edge1 = face.Edge;
                HalfEdge2 edge2 = edge1.NextEdge;
                HalfEdge2 edge3 = edge2.NextEdge;

                //Calculate the circumcenter for this triangle
                Vector2 vector1 = edge1.Vertex.Position;
                Vector2 vector2 = edge2.Vertex.Position;
                Vector2 vector3 = edge3.Vertex.Position;

                //The circumcenter is the center of a circle where the triangles corners is on the circumference of that circle
                //The circumcenter is also known as a voronoi vertex, which is a position in the diagram where we are equally
                //close to the surrounding sites (= the corners ina voronoi cell)
                Vector2 voronoiVertex = CalculateCircleCenter(new(vector1, vector2, vector3));

                //We will generate a single edge belonging to this site
                //Try means that this edge might not have an opposite and then we can't generate an edge
                TryAddVoronoiEdgeFromTriangleEdge(edge1, voronoiVertex, voronoiEdges);
                TryAddVoronoiEdgeFromTriangleEdge(edge2, voronoiVertex, voronoiEdges);
                TryAddVoronoiEdgeFromTriangleEdge(edge3, voronoiVertex, voronoiEdges);
            }

            //Step 2. Find the voronoi cells where each cell is a list of all edges belonging to a site
            //So we have a lot of edges and now each edge should get a cell
            //These edges are not sorted, so they are added as we find them
            VoronoiCells.Clear();

            for (int i = 0; i < voronoiEdges.Count; i++)
            {
                VoronoiEdge2 edge = voronoiEdges[i];

                //Find the cell in the list of all cells that includes this site
                VoronoiCell2 cell = TryFindCell(edge, VoronoiCells);

                //No cell was found so we need to create a new cell
                if (cell == null)
                {
                    VoronoiCell2 newCell = new(edge.SitePosition);

                    VoronoiCells.Add(newCell);

                    newCell.Edges.Add(edge);
                }
                else
                {
                    cell.Edges.Add(edge);
                }
            }
        }

        private static Vector2 CalculateCircleCenter(Triangle2 triangle)
        {
            //Make sure the triangle a-b-c is counterclockwise
            triangle = triangle.OrientClockwise();

            //The area of the triangle
            float x1 = triangle.Point2.x - triangle.Point1.x;
            float x2 = triangle.Point3.x - triangle.Point1.x;
            float y1 = triangle.Point2.y - triangle.Point1.y;
            float y2 = triangle.Point3.y - triangle.Point1.y;

            float A = 0.5f * Det2(x1, y1, x2, y2);

            //Debug.Log(A);


            //The center coordinates:
            //float L_10 = MyVector2.Magnitude(b - a);
            //float L_20 = MyVector2.Magnitude(c - a);

            //float L_10_square = L_10 * L_10;
            //float L_20_square = L_20 * L_20;

            float L_10_square = Vector2.SqrMagnitude(triangle.Point2 - triangle.Point1);
            float L_20_square = Vector2.SqrMagnitude(triangle.Point3 - triangle.Point1);

            float one_divided_by_4A = 1f / (4f * A);

            float x = triangle.Point1.x + one_divided_by_4A * ((y2 * L_10_square) - (y1 * L_20_square));
            float y = triangle.Point1.y + one_divided_by_4A * ((x1 * L_20_square) - (x2 * L_10_square));
            Vector2 center = new(x, y);

            return center;
        }

        // Returns the determinant of the 2x2 matrix defined as
        // | x1 x2 |
        // | y1 y2 |
        //det(a_normalized, b_normalized) = sin(alpha) so it's similar to the dot product
        //Vector alignment dot det
        //Same:            1   0
        //Perpendicular:   0  -1
        //Opposite:       -1   0
        //Perpendicular:   0   1
        private static float Det2(float x1, float x2, float y1, float y2)
        {
            return x1 * y2 - y1 * x2;
        }

        //Try to add a voronoi edge. Not all edges have a neighboring triangle, and if it hasnt we cant add a voronoi edge
        private static void TryAddVoronoiEdgeFromTriangleEdge(HalfEdge2 halfEdge, Vector2 voronoiVertex, List<VoronoiEdge2> allEdges)
        {
            //Ignore if this edge has no neighboring triangle
            //If no opposite exists, we could maybe add a fake opposite to get an edge far away
            if (halfEdge.OppositeEdge == null)
            {
                return;
            }

            //Calculate the circumcenter of the neighbor
            HalfEdge2 eNeighbor = halfEdge.OppositeEdge;

            Vector2 v1 = eNeighbor.Vertex.Position;
            Vector2 v2 = eNeighbor.NextEdge.Vertex.Position;
            Vector2 v3 = eNeighbor.NextEdge.NextEdge.Vertex.Position;

            Vector2 voronoiVertexNeighbor = CalculateCircleCenter(new(v1, v2, v3));

            //Create a new voronoi edge between the voronoi vertices
            //Each edge in the half-edge data structure points TO a vertex, so this edge will be associated
            //with the vertex the edge is going from
            VoronoiEdge2 edge = new(voronoiVertex, voronoiVertexNeighbor, sitePosistion: halfEdge.PreviousEdge.Vertex.Position);

            allEdges.Add(edge);
        }

        //Find the cell in the list of all cells that includes this site
        private static VoronoiCell2 TryFindCell(VoronoiEdge2 edge, HashSet<VoronoiCell2> voronoiCells)
        {
            foreach (VoronoiCell2 cell in voronoiCells)
                if (edge.SitePosition.Equals(cell.SitePosition))
                    return cell;

            return null;
        }
    }

    public class VoronoiEdge2
    {
        public Vector2 Point1;
        public Vector2 Point2;

        //All positions within a voronoi cell is closer to this position than any other position in the diagram
        //Is also a vertex in the delaunay triangulation
        public Vector2 SitePosition;
        public VoronoiEdge2(Vector2 point1, Vector2 point2, Vector2 sitePosistion)
        {
            Point1 = point1;
            Point2 = point2;

            SitePosition = sitePosistion;
        }
    }

    public class VoronoiCell2
    {
        //All positions within a voronoi cell is closer to this position than any other position in the diagram
        //Is also a vertex in the delaunay triangulation
        public Vector2 SitePosition;

        public List<VoronoiEdge2> Edges = new();

        public VoronoiCell2(Vector2 sitePos)
        {
            this.SitePosition = sitePos;
        }
    }
}
