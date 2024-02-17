using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Retrover.Math
{

    [TestFixture]
    public class VoronoiDiagramTests
    {
        private List<Vector2> GenerateRandomPoints(int count)
        {
            List<Vector2> points = new List<Vector2>();
            for (int i = 0; i < count; i++)
            {
                points.Add(new Vector2(Random.Range(-100f, 100f), Random.Range(-100f, 100f)));
            }
            return points;
        }

        [Test]
        public void CellClosureTest()
        {
            List<Vector2> points = GenerateRandomPoints(10);
            VoronoiDiagram diagram = new(new PointsGroup(points));
            diagram.Calculate();

            foreach (var cell in diagram.Cells)
            {
                Edge firstEdge = cell.Edges.First();
                Edge lastEdge = cell.Edges.Last();

                Assert.AreEqual(firstEdge.Start, lastEdge.End, "Edges of the cell should form a closed loop.");
            }
        }

        [Test]
        public void EdgeBelongsToOneOrTwoCellsTest()
        {
            List<Vector2> points = GenerateRandomPoints(10);
            VoronoiDiagram diagram = new(new PointsGroup(points));
            diagram.Calculate();

            Dictionary<Edge, List<VoronoiCell>> edgeToCellsMap = new();

            foreach (VoronoiCell cell in diagram.Cells)
            {
                foreach (Edge edge in cell.Edges)
                {
                    if (!edgeToCellsMap.ContainsKey(edge))
                        edgeToCellsMap[edge] = new List<VoronoiCell>();

                    edgeToCellsMap[edge].Add(cell);
                }
            }
            foreach (KeyValuePair<Edge, List<VoronoiCell>> edgeCells in edgeToCellsMap)
            {
                Assert.IsTrue(edgeCells.Value.Count <= 2, $"Edge should belong to one or two cells, but found {edgeCells.Value.Count}");
            }
        }
    }
}
