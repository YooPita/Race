using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Retrover.Math
{
    public class VertexTriangleGroup : IEnumerable<KeyValuePair<Vector2, List<Triangle>>>
    {
        private readonly Dictionary<Vector2, List<Triangle>> _map = new();
        private readonly IEnumerable<Vector2> _points;
        private readonly IEnumerable<Triangle> _triangles;

        public VertexTriangleGroup(IEnumerable<Vector2> points, IEnumerable<Triangle> triangles)
        {
            _points = points;
            _triangles = triangles;
        }

        public void Calculate()
        {
            foreach (var point in _points)
            {
                foreach (var triangle in _triangles)
                {
                    if (TriangleContainsPoint(triangle, point))
                    {
                        if (!_map.ContainsKey(point))
                        {
                            _map[point] = new List<Triangle>();
                        }
                        _map[point].Add(triangle);
                    }
                }
            }
        }

        public IEnumerator<KeyValuePair<Vector2, List<Triangle>>> GetEnumerator()
        {
            return _map.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private bool TriangleContainsPoint(Triangle triangle, Vector2 point)
        {
            return point == triangle.Vertex1 || point == triangle.Vertex2 || point == triangle.Vertex3;
        }
    }
}
