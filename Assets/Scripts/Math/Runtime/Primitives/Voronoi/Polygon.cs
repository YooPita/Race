using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace Retrover.Math
{
    public class Polygon : IPolygon
    {
        public ReadOnlyCollection<Edge> Edges => _edges.AsReadOnly();

        private readonly List<Edge> _edges = new();

        public Polygon(List<Edge> edges)
        {
            if (edges.Count < 3)
                throw new ArgumentException("Polygon must have at least three edges", nameof(edges));

            _edges.AddRange(edges);
        }

        public Polygon(List<Vector2> points)
        {
            if (points == null || points.Count < 3)
                throw new ArgumentException("Polygon must have at least three points", nameof(points));

            for (int i = 0; i < points.Count; i++)
            {
                Vector2 start = points[i];
                Vector2 end = points[(i + 1) % points.Count];
                _edges.Add(new Edge(start, end));
            }
        }

        public bool ContainsPoint(Vector2 point)
        {
            return IPolygon.PointInPolygon(point, this);
        }
    }
}
