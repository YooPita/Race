using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;

namespace Retrover.Math
{
    public class RectanglePointsGroup : IPointsGroup
    {
        private Rectangle _rectangle;
        private IEnumerable<Vector2> _points;
        private Triangle _superTriangle;

        public RectanglePointsGroup(Rectangle rectangle, IEnumerable<Vector2> points)
        {
            _rectangle = rectangle;
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

            float minX = _rectangle.X;
            float minY = _rectangle.Y;

            float width = _rectangle.Width;
            float height = _rectangle.Height;

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
