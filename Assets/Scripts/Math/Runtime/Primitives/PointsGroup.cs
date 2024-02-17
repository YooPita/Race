using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Retrover.Math
{
    public class PointsGroup : IPointsGroup
    {
        private IEnumerable<Vector2> _points;
        private Triangle _superTriangle;

        public PointsGroup(IEnumerable<Vector2> points)
        {
            _points = points;
        }

        public void RemoveDuplicates()
        {
            _points = _points.Distinct();
        }

        public Triangle SuperTriangle()
        {
            if (_superTriangle != null)
                return _superTriangle;

            if (!_points.Any())
                throw new ArgumentException("List of points cannot be empty");

            float minX = float.MaxValue;
            float maxX = float.MinValue;
            float minY = float.MaxValue;
            float maxY = float.MinValue;

            foreach (Vector2 point in _points)
            {
                if (point.x < minX) minX = point.x;
                if (point.x > maxX) maxX = point.x;
                if (point.y < minY) minY = point.y;
                if (point.y > maxY) maxY = point.y;
            }

            float width = maxX - minX;
            float height = maxY - minY;

            minX -= width / 2f;
            minY -= height / 2f;
            width *= 3f;
            height *= 3f;

            Vector2 point1 = new(minX, minY);
            Vector2 point2 = new(minX + width, minY);
            Vector2 point3 = new(minX, minY + height);

            _superTriangle = new Triangle(point1, point2, point3);
            return _superTriangle;
        }

        public IEnumerator<Vector2> GetEnumerator()
        {
            return _points.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
