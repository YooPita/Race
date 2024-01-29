using System;
using System.Linq;

namespace Retrover.Math
{
    public class Delaunay
    {
        public TriangulationData TriangulationData { get; private set; }

        private readonly PointArea _pointArea;

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
            TriangulationData = triangulation.Data;
        }

        private bool CanCalculate()
        {
            return _pointArea.Points.Count >= 2;
        }
    }
}
