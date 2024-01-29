using System;

namespace Retrover.Math
{
    public class Delaunay
    {
        public Triangulation Triangulation { get; private set; }

        private readonly PointArea _pointArea;

        public Delaunay(PointArea pointArea)
        {
            _pointArea = pointArea;
            Triangulation = new Triangulation(_pointArea.Points);
        }

        public void Calculate()
        {
            if (!CanCalculate())
            {
                throw new InvalidOperationException("Not enough points for calculation. Minimum 2 points required.");
            }

            Triangulation.Calculate();
        }

        private bool CanCalculate()
        {
            return _pointArea.Points.Count >= 2;
        }
    }
}
