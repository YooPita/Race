using System.Collections.Generic;
using UnityEngine;

namespace Retrover.Math
{
    public class Triangulation
    {
        public TriangulationData Data { get; private set; }

        private readonly TriangulationCalculator _calculator;

        public Triangulation(HashSet<Vector2> points)
        {
            Data = new TriangulationData(points);
            _calculator = new TriangulationCalculator(Data);
        }

        public void Calculate()
        {
            _calculator.Calculate();
        }
    }
}
