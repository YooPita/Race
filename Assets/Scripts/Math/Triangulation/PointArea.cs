using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Retrover.Math
{
    public readonly struct PointArea
    {
        public Rect Area { get; }
        public HashSet<Vector2> Points { get; }

        public PointArea(Rect area, HashSet<Vector2> points)
        {
            Area = area;
            Points = points;
        }

        public IEnumerable<Vector2> Normalize()
        {
            return Points.Where(IsPointInsideArea).Select(NormalizedPoint);
        }

        public IEnumerable<Vector2> Denormalize(IEnumerable<Vector2> normalizedPoints)
        {
            return normalizedPoints.Select(DenormalizePoint);
        }

        private bool IsPointInsideArea(Vector2 point) => Area.Contains(point);

        private Vector2 NormalizedPoint(Vector2 point)
        {
            float normalizedX = (point.x - Area.xMin) / Area.width;
            float normalizedY = (point.y - Area.yMin) / Area.height;
            return new Vector2(normalizedX, normalizedY);
        }

        private Vector2 DenormalizePoint(Vector2 point)
        {
            float denormalizedX = point.x * Area.width + Area.xMin;
            float denormalizedY = point.y * Area.height + Area.yMin;
            return new Vector2(denormalizedX, denormalizedY);
        }
    }
}
