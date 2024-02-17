using System.Collections.Generic;
using UnityEngine;

namespace Retrover.Math
{
    public class VoronoiDiagram
    {
        public List<VoronoiCell> Cells { get; private set; } = new();
        public VertexTriangleGroup Triangles { get; private set; }
        private readonly IPointsGroup _sites;

        public VoronoiDiagram(IPointsGroup sites)
        {
            _sites = sites;
        }

        public void Calculate()
        {
            if (Cells.Count > 0)
                return;

            _sites.RemoveDuplicates();
            DelaunayTriangulation delaunayTriangulation = new(_sites);
            delaunayTriangulation.Calculate();
            Triangles = new(_sites, delaunayTriangulation.Triangles);
            Triangles.Calculate();
            CalculateCells(Triangles);
            RemoveCellsBelongingToSuperTriangle(_sites.SuperTriangle());
        }

        private void CalculateCells(VertexTriangleGroup triangles)
        {
            foreach (KeyValuePair<Vector2, List<Triangle>> pointTrianglesPair in triangles)
            {
                Vector2 point = pointTrianglesPair.Key;
                List<Vector2> centers = new();

                foreach (Triangle triangle in pointTrianglesPair.Value)
                    centers.Add(triangle.Circumcircle.Center);

                List<Vector2> centersWithoutDuplicates = RemoveDuplicateCenters(centers, 0.001f);
                List<Vector2> sortedCenters = SortCentersByAngle(point, centersWithoutDuplicates);

                VoronoiCell cell = CreateEdgesForCell(point, sortedCenters);

                Cells.Add(cell);
            }
        }

        private static List<Vector2> SortCentersByAngle(Vector2 point, List<Vector2> centers)
        {
            centers.Sort((center1, center2) =>
            {
                float angle1 = Mathf.Atan2(center1.y - point.y, center1.x - point.x);
                float angle2 = Mathf.Atan2(center2.y - point.y, center2.x - point.x);
                return angle1.CompareTo(angle2);
            });

            return centers;
        }

        private static VoronoiCell CreateEdgesForCell(Vector2 point, List<Vector2> sortedCenters)
        {
            VoronoiCell cell = new(point);
            for (int i = 0; i < sortedCenters.Count; i++)
            {
                Vector2 start = sortedCenters[i];
                Vector2 end = sortedCenters[(i + 1) % sortedCenters.Count];
                cell.Edges.Add(new Edge(start, end));
            }
            return cell;
        }

        private void RemoveCellsBelongingToSuperTriangle(Triangle superTriangle)
        {
            Vector2 vertex1 = superTriangle.Vertex1;
            Vector2 vertex2 = superTriangle.Vertex2;
            Vector2 vertex3 = superTriangle.Vertex3;

            Cells.RemoveAll(cell => cell.Site == vertex1 || cell.Site == vertex2 || cell.Site == vertex3);
        }

        private List<Vector2> RemoveDuplicateCenters(List<Vector2> centers, float tolerance)
        {
            List<Vector2> uniqueCenters = new();

            foreach (var center in centers)
            {
                bool isDuplicate = false;
                foreach (var uniqueCenter in uniqueCenters)
                {
                    if (Vector2.Distance(center, uniqueCenter) < tolerance)
                    {
                        isDuplicate = true;
                        break;
                    }
                }
                if (!isDuplicate)
                {
                    uniqueCenters.Add(center);
                }
            }

            return uniqueCenters;
        }
    }
}