using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace Retrover.Math
{
    [TestFixture]
    public class DelaunayTriangulationTests
    {
        private List<Vector2> GenerateRandomPoints(int count)
        {
            List<Vector2> points = new();

            for (int i = 0; i < count; i++)
                points.Add(new(Random.Range(-100f, 100f), Random.Range(-100f, 100f)));

            return points;
        }

        [Test]
        public void DelaunayConditionTest()
        {
            List<Vector2> points = GenerateRandomPoints(100);
            PointsGroup pointGroup = new(points);
            pointGroup.RemoveDuplicates();
            DelaunayTriangulation delaunayTriangulation = new(pointGroup);
            delaunayTriangulation.Calculate();

            foreach (var triangle in delaunayTriangulation.Triangles)
            {
                foreach (var point in points)
                {
                    if (!triangle.Vertex1.Equals(point) && !triangle.Vertex2.Equals(point) && !triangle.Vertex3.Equals(point))
                    {
                        Assert.IsFalse(triangle.ContainsInCircumcircle(point), "Delaunay condition violated");
                    }
                }
            }
        }

        [Test]
        public void CompletenessTest()
        {
            var points = GenerateRandomPoints(100);
            PointsGroup pointGroup = new(points);
            pointGroup.RemoveDuplicates();
            var delaunayTriangulation = new DelaunayTriangulation(pointGroup);
            delaunayTriangulation.Calculate();

            foreach (var point in points)
            {
                bool found = false;
                foreach (var triangle in delaunayTriangulation.Triangles)
                {
                    if (triangle.Vertex1.Equals(point) || triangle.Vertex2.Equals(point) || triangle.Vertex3.Equals(point))
                    {
                        found = true;
                        break;
                    }
                }
                Assert.IsTrue(found, "Completeness test failed: a point was not found in any triangle");
            }
        }

        [Test]
        public void NoEdgeIntersectionsTest()
        {
            var points = GenerateRandomPoints(100);
            PointsGroup pointGroup = new(points);
            pointGroup.RemoveDuplicates();
            var delaunayTriangulation = new DelaunayTriangulation(pointGroup);
            delaunayTriangulation.Calculate();
            var triangles = delaunayTriangulation.Triangles;

            for (int i = 0; i < triangles.Count; i++)
            {
                for (int j = i + 1; j < triangles.Count; j++)
                {
                    var edgesI = new List<Edge> { triangles[i].Edge1, triangles[i].Edge2, triangles[i].Edge3 };
                    var edgesJ = new List<Edge> { triangles[j].Edge1, triangles[j].Edge2, triangles[j].Edge3 };

                    foreach (var edgeI in edgesI)
                    {
                        foreach (var edgeJ in edgesJ)
                        {
                            if (!edgeI.Start.Equals(edgeJ.Start) && !edgeI.Start.Equals(edgeJ.End) &&
                                !edgeI.End.Equals(edgeJ.Start) && !edgeI.End.Equals(edgeJ.End))
                            {
                                Assert.IsFalse(DoEdgesIntersect(edgeI.Start, edgeI.End, edgeJ.Start, edgeJ.End),
                                    "Edges should not intersect");
                            }
                        }
                    }
                }
            }
        }

        private bool DoEdgesIntersect(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            float Area(Vector2 p1, Vector2 p2, Vector2 p3)
            {
                return (p2.x - p1.x) * (p3.y - p1.y) - (p3.x - p1.x) * (p2.y - p1.y);
            }

            bool Intersect1D(float a, float b, float c, float d)
            {
                if (a > b)
                    (b, a) = (a, b);

                if (c > d)
                    (d, c) = (c, d);

                return Mathf.Max(a, c) <= Mathf.Min(b, d);
            }

            if (!Intersect1D(a.x, b.x, c.x, d.x) || !Intersect1D(a.y, b.y, c.y, d.y))
                return false;

            if (Area(a, b, c) == 0 && Area(a, b, d) == 0)
                return Intersect1D(a.x, b.x, c.x, d.x) && Intersect1D(a.y, b.y, c.y, d.y);

            return Area(a, b, c) * Area(a, b, d) <= 0 && Area(c, d, a) * Area(c, d, b) <= 0;
        }
    }
}
