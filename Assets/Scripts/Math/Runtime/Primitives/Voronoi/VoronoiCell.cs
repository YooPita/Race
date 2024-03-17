using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace Retrover.Math
{
    public class VoronoiCell : IPolygon
    {
        public Vector2 Site { get; private set; }
        public ReadOnlyCollection<(Edge edge, VoronoiCell neighbor)> EdgesWithNeighbors => _edgesWithNeighbors.AsReadOnly();
        public ReadOnlyCollection<Edge> Edges => _lazyEdges.Value;

        private readonly Lazy<ReadOnlyCollection<Edge>> _lazyEdges;
        private readonly List<(Edge edge, VoronoiCell neighbor)> _edgesWithNeighbors = new();
        private int? _cachedHashCode;

        public VoronoiCell(Vector2 site)
        {
            Site = site;
            _lazyEdges = new Lazy<ReadOnlyCollection<Edge>>(() => new ReadOnlyCollection<Edge>(_edgesWithNeighbors.ConvertAll(pair => pair.edge)));
        }

        public void AddEdge(Edge edge, VoronoiCell neighbor)
        {
            _edgesWithNeighbors.Add((edge, neighbor));
        }

        public bool ContainsPoint(Vector2 point)
        {
            return IPolygon.PointInPolygon(point, this);
        }
    }
}