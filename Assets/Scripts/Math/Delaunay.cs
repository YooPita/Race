using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Retrover.Math
{
    public class Delaunay
    {
        public List<Triangle2> Triangles { get; private set; } = new();

        private readonly PointArea _pointArea;
        private TriangulationData _triangulationData;

        public Delaunay(PointArea pointArea)
        {
            _pointArea = pointArea;
        }

        public void Calculate()
        {
            if (!CanCalculate())
            {
                throw new InvalidOperationException("Not enough points for calculation. Minimum 2 points required.");
            }

            Triangulation triangulation = new(_pointArea.Normalize().ToHashSet());
            triangulation.Calculate();
            _triangulationData = triangulation.Data;
            Triangles.Clear();

            foreach(HalfEdgeFace2 face in _triangulationData.Faces)
            {
                IEnumerable<HalfEdgeVertex2> verticles = face.GetVertices();
                Vector2[] points = _pointArea.Denormalize(verticles.Select(x => x.Position)).ToArray();
                Triangles.Add(new(points[0], points[1], points[2]));
            }
        }

        private bool CanCalculate()
        {
            return _pointArea.Points.Count >= 2;
        }
    }
}
