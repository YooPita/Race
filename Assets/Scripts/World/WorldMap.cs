using Reflex.Attributes;
using Reflex.Core;
using Retrover.Math;
using Retrover.Path;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WorldMap
{
    private readonly int _chunkSize;
    private readonly int _pointsPerChunk;
    private readonly int _worldSeed;

    public WorldMap(int seed, int chunkSize = 100, int pointsPerChunk = 10)
    {
        _worldSeed = seed;
        _chunkSize = chunkSize;
        _pointsPerChunk = pointsPerChunk;
    }

    public Vector2Int CurrentChunk(Vector2 currentPosition)
    {
        return new Vector2Int(
            Mathf.FloorToInt(currentPosition.x / _chunkSize),
            Mathf.FloorToInt(currentPosition.y / _chunkSize)
        );
    }

    public List<Vector2> ChunkPoints(int chunkX, int chunkY)
    {
        int chunkSeed = GetChunkSeed(chunkX, chunkY);
        System.Random chunkRandom = new(chunkSeed);

        List<Vector2> points = new();
        float segmentSize = (float)_chunkSize / _pointsPerChunk;

        for (int x = 0; x < _pointsPerChunk; x++)
        {
            for (int y = 0; y < _pointsPerChunk; y++)
            {
                float pointX = (chunkX * _chunkSize) + (x * segmentSize) + (NextFloat(chunkRandom) * segmentSize);
                float pointY = (chunkY * _chunkSize) + (y * segmentSize) + (NextFloat(chunkRandom) * segmentSize);
                points.Add(new Vector2(pointX, pointY));
            }
        }
        return points;
    }

    public List<Vector2> ChunkRoadPoints(int chunkX, int chunkY)
    {
        int chunkSeed = GetChunkSeed(chunkX, chunkY);
        System.Random chunkRandom = new(chunkSeed);

        int minRoadPoints = 1;
        int maxRoadPoints = 3;
        int roadPointsCount = chunkRandom.Next(minRoadPoints, maxRoadPoints + 1);

        List<Vector2> roadPoints = new();
        for (int i = 0; i < roadPointsCount; i++)
        {
            float x = chunkRandom.Next(chunkX * _chunkSize, (chunkX + 1) * _chunkSize);
            float y = chunkRandom.Next(chunkY * _chunkSize, (chunkY + 1) * _chunkSize);
            roadPoints.Add(new Vector2(x, y));
        }

        return roadPoints;
    }


    private float NextFloat(System.Random rnd)
    {
        return (float)rnd.NextDouble();
    }

    private int GetChunkSeed(int chunkX, int chunkY)
    {
        return (chunkX * 73856093) ^ (chunkY * 19349663) ^ _worldSeed;
    }

}


public class WorldStream : IWorldFocusPoint, IWorldStreamPublisher
{
    public List<VoronoiCell> Cells { get; private set; } = new();
    public VoronoiCell CurrentCell => _currentCell;
    public Vector2 Position { get; private set; }

    private Vector2Int _currentChunk;
    private bool _initialized = false;
    [Inject] private readonly WorldMap _worldMap;
    private int _size = 0;
    private VoronoiCell _currentCell;
    private Subscription<IWorldRenderer> _subscribers = new();

    public WorldStream(int size = 1)
    {
        _size = Mathf.Abs(size);
    }

    public void UpdatePosition(Vector2 currentPosition)
    {
        Position = currentPosition;
        Vector2Int newChunk = _worldMap.CurrentChunk(currentPosition);

        if (newChunk != _currentChunk || !_initialized)
        {
            _currentChunk = newChunk;
            UpdateCells();
            _initialized = true;
        }

        CalculateCurrentCell(currentPosition);
    }

    private void UpdateCells()
    {
        List<Vector2> points = new();
        List<Vector2> roadPoints = new();
        for (int dx = -_size; dx <= _size; dx++)
        {
            for (int dy = -_size; dy <= _size; dy++)
            {
                Vector2Int chunkCoords = new(_currentChunk.x + dx, _currentChunk.y + dy);
                List<Vector2> chunkPoints = _worldMap.ChunkPoints(chunkCoords.x, chunkCoords.y);
                points.AddRange(chunkPoints);
                List<Vector2> chunkRoadPoints = _worldMap.ChunkRoadPoints(chunkCoords.x, chunkCoords.y);
                roadPoints.AddRange(chunkRoadPoints);
            }
        }

        VoronoiDiagram diagram = new(points);
        diagram.Calculate();
        Cells.Clear();
        Cells.AddRange(diagram.Cells);
    }

    private void CalculateCurrentCell(Vector2 currentPosition)
    {
        if (_currentCell == null || !_currentCell.ContainsPoint(currentPosition))
        {
            FindCurrentCell(currentPosition);
        }
    }

    private void FindCurrentCell(Vector2 currentPosition)
    {
        List<VoronoiCell> sortedCells = Cells.OrderBy(cell => (cell.Site - currentPosition).sqrMagnitude).ToList();

        foreach (VoronoiCell cell in sortedCells)
        {
            if (cell.ContainsPoint(currentPosition))
            {
                if (_currentCell != cell)
                {
                    _currentCell = cell;
                    NotifySubscribers();
                    break;
                }
            }
        }
    }

    public void Subscribe(IWorldRenderer subscriber)
    {
        _subscribers.Subscribe(subscriber);
    }

    public void Unsubscribe(IWorldRenderer subscriber)
    {
        _subscribers.Unsubscribe(subscriber);
    }

    private void NotifySubscribers()
    {
        foreach (IWorldRenderer subscriber in _subscribers)
        {
            subscriber.Update();
        }
    }
}

public interface IWorldFocusPoint
{
    Vector2 Position { get; }
    void UpdatePosition(Vector2 newPosition);
}

public class WorldViewport : IWorldFocusPoint
{
    public Vector2 Position { get; private set; }

    [Inject] private readonly WorldStream _stream;
    private Vector2 _lastPosition;
    private float _minDistance;
    private bool _dirty = true;

    public WorldViewport(float minDistance)
    {
        _minDistance = minDistance;
    }

    public void UpdatePosition(Vector2 newPosition)
    {
        Position = newPosition;
        if (_dirty || Vector2.Distance(_lastPosition, newPosition) > _minDistance)
        {
            _lastPosition = newPosition;
            _dirty = false;
            _stream.UpdatePosition(_lastPosition);
        }
    }
}

public interface IWorldRenderer
{
    void Update();
}

public interface IWorldStreamPublisher
{
    void Subscribe(IWorldRenderer subscriber);
    void Unsubscribe(IWorldRenderer subscriber);
}

public class RoadRenderer : IWorldRenderer, IInitializable
{
    [Inject] private readonly WorldStream _stream;
    [Inject] private readonly RoadChunkFactory _roadFactory;
    private List<CellToChunk> _chunks = new();
    private readonly int _worldSeed;

    public RoadRenderer(int seed)
    {
        _worldSeed = seed;
    }

    public void Initialize()
    {
        _stream.Subscribe(this);
    }

    public void Update()
    {
        if (_chunks.Count == 0)
            SetStartCell(_stream.CurrentCell);
        else if (_stream.CurrentCell.Site == _chunks[0].Cell.Site)
        {
            RemoveCellAtEnd();
            AddCellToStart(_stream.CurrentCell);
        }
        else if (_stream.CurrentCell.Site == _chunks[^1].Cell.Site)
        {
            RemoveCellAtStart();
            AddCellToEnd(_stream.CurrentCell);
        }
    }

    private void SetStartCell(VoronoiCell cell)
    {
        CellToChunk currentChunk = new(cell, RenderChunk(cell));
        currentChunk.View.Render();
        _chunks.Add(currentChunk);
        AddCellToEnd(cell);
        AddCellToStart(cell);
    }

    private void AddCellToEnd(VoronoiCell cell)
    {
        VoronoiCell nextCell = FindCell(_chunks[^1].View.EndEdge, cell);
        CellToChunk nextChunk = new(nextCell, RenderChunk(nextCell, _chunks[^1].View.EndEdge, true));
        nextChunk.View.Render();
        _chunks.Add(nextChunk);
    }

    private void AddCellToStart(VoronoiCell cell)
    {
        VoronoiCell previousCell = FindCell(_chunks[0].View.StartEdge, cell);
        CellToChunk previousChunk = new(previousCell, RenderChunk(previousCell, _chunks[0].View.StartEdge));
        previousChunk.View.Render();
        _chunks.Insert(0, previousChunk);
    }

    private void RemoveCellAtEnd()
    {
        _chunks[^1].View.Remove();
        _chunks.RemoveAt(_chunks.Count - 1);
    }

    private void RemoveCellAtStart()
    {
        _chunks[0].View.Remove();
        _chunks.RemoveAt(0);
    }

    private VoronoiCell FindCell(Edge edge, VoronoiCell cell)
    {
        foreach (var (currentEdge, neighbor) in cell.EdgesWithNeighbors)
        {
            if (currentEdge.Equals(edge))
            {
                return neighbor;
            }
        }
        return null;
    }

    private RoadChunkView RenderChunk(VoronoiCell cell, Edge? existedEdge = null, bool isStart = false)
    {
        int chunkSeed = GetPointSeed(cell.Site);
        System.Random chunkRandom = new(chunkSeed);

        List<Edge> edges = cell.Edges.ToList();

        int firstEdgeIndex = chunkRandom.Next(edges.Count);
        Edge startEdge = existedEdge ?? edges[firstEdgeIndex];

        edges = edges.Where(e => !IsEdgeConnected(e, startEdge)).ToList();

        if (!edges.Any())
            throw new InvalidOperationException("Cannot select side two due to no sides available.");

        Edge endEdge = edges[chunkRandom.Next(edges.Count)];

        if (existedEdge != null & !isStart)
            return _roadFactory.NewRoadChunkView(cell, endEdge, startEdge);
        return _roadFactory.NewRoadChunkView(cell, startEdge, endEdge);
    }

    private bool IsEdgeConnected(Edge edge, Edge startEdge)
    {
        return edge.Start == startEdge.Start || edge.Start == startEdge.End
            || edge.End == startEdge.Start || edge.End == startEdge.End;
    }


    private int GetPointSeed(Vector2 point)
    {
        int x = Mathf.RoundToInt(point.x * 1000);
        int y = Mathf.RoundToInt(point.y * 1000);

        return (x * 73856093) ^ (y * 19349663) ^ _worldSeed;
    }

    private struct CellToChunk
    {
        public VoronoiCell Cell { get; private set; }
        public RoadChunkView View { get; private set; }

        public CellToChunk(VoronoiCell cell, RoadChunkView view)
        {
            Cell = cell;
            View = view;
        }
    }
}

public class RoadChunkFactory
{
    [Inject] private readonly Container _container;

    public RoadChunkView NewRoadChunkView(VoronoiCell cell, Edge startEdge, Edge endEdge)
    {
        RoadChunkView roadChunkView = _container.Resolve<RoadChunkView>();
        roadChunkView.Initialize(cell, startEdge, endEdge);
        return roadChunkView;
    }
}

public class RoadChunkView
{
    public Edge StartEdge { get; private set; }
    public Edge EndEdge { get; private set; }
    private VoronoiCell _cell;

    [Inject] private readonly RoadMeshesPool _roadPool;
    [Inject] private readonly PathBakeOptions _pathBakeOptions;
    private int _worldSeed;
    private bool _rendered = false;
    private RoadMeshNew _mesh;

    public RoadChunkView(int seed)
    {
        _worldSeed = seed;
    }

    public void Initialize(VoronoiCell cell, Edge startEdge, Edge endEdge)
    {
        _cell = cell;
        StartEdge = startEdge;
        EndEdge = endEdge;
    }

    public void Render()
    {
        if (_rendered)
            return;

        int chunkSeed = GetPointSeed(_cell.Site);

        Vector2 startCenter = (StartEdge.Start + StartEdge.End) / 2;
        Vector2 startCenterNormal = startCenter + (FindInsideNormal(StartEdge, startCenter) * 20);

        Vector2 endCenter = (EndEdge.Start + EndEdge.End) / 2;
        Vector2 endCenterNormal = endCenter - (FindInsideNormal(EndEdge, endCenter) * 20);

        System.Random chunkRandom = new(chunkSeed);
        List<Vector2> randomPoints = GenerateRandomPointsInsidePolygon(chunkRandom, startCenter, endCenter);

        List<Vector2> roadPoints = GenerateRoadPoints(startCenter, endCenter, randomPoints);

        Path path = new(_pathBakeOptions);

        SymmetricNode startNode = new(new Vector3(startCenter.x, 0, startCenter.y), new(startCenterNormal.x, 0, startCenterNormal.y));
        SymmetricNode endNode = new(new Vector3(endCenter.x, 0, endCenter.y), new(endCenterNormal.x, 0, endCenterNormal.y));

        path.AddNode(startNode);

        for (int i = 1; i < roadPoints.Count - 1; i++)
        {
            Vector2 directionToCurrent = (roadPoints[i] - roadPoints[i - 1]).normalized;
            Vector2 directionToNext = (roadPoints[i + 1] - roadPoints[i]).normalized;
            Vector2 averageDirection = ((directionToCurrent + directionToNext) / 2).normalized;

            Vector2 direction = roadPoints[i] + averageDirection * 20;

            SymmetricNode roadNode = new(new Vector3(roadPoints[i].x, 0, roadPoints[i].y), new Vector3(direction.x, 0, direction.y));

            path.AddNode(roadNode);
        }

        path.AddNode(endNode);
        path.Bake();

        List<PathTransform> transforms = new();

        for (int i = 0; i < path.SegmentsCount; i++)
        {
            ISegment segment = path.SegmentAtIndex(i);
            transforms.AddRange(segment.TransformsCopy());
        }

        _mesh = _roadPool.RoadMesh(transforms, 8f);

        _rendered = true;
    }

    public void Remove()
    {
        if (!_rendered)
            return;

        _roadPool.Push(_mesh);
        _rendered = false;
    }

    private List<Vector2> GenerateRandomPointsInsidePolygon(System.Random chunkRandom, Vector2 start, Vector2 end)
    {
        List<Vector2> randomPoints = new();
        List<Edge> edges = _cell.Edges.ToList();

        float minX = edges.Min(e => Math.Min(e.Start.x, e.End.x));
        float maxX = edges.Max(e => Math.Max(e.Start.x, e.End.x));
        float minY = edges.Min(e => Math.Min(e.Start.y, e.End.y));
        float maxY = edges.Max(e => Math.Max(e.Start.y, e.End.y));

        int pointsCount = chunkRandom.Next(1, 6);

        for (int i = 0; i < pointsCount; i++)
        {
            float x = (float)(minX + chunkRandom.NextDouble() * (maxX - minX));
            float y = (float)(minY + chunkRandom.NextDouble() * (maxY - minY));
            var newPoint = new Vector2(x, y);

            if (!_cell.ContainsPoint(newPoint))
                continue;

            bool tooCloseToEdge = edges.Any(edge => Vector2.Distance(edge.ProjectedPoint(newPoint), newPoint) < 15f);

            bool tooCloseToPoint = randomPoints.Any(p => Vector2.Distance(p, newPoint) < 10f);

            tooCloseToPoint = tooCloseToPoint || Vector2.Distance(newPoint, start) < 20f;
            tooCloseToPoint = tooCloseToPoint || Vector2.Distance(newPoint, end) < 20f;

            if (!tooCloseToEdge && !tooCloseToPoint)
                randomPoints.Add(newPoint);
        }

        return randomPoints;
    }

    private int GetPointSeed(Vector2 point)
    {
        int x = Mathf.RoundToInt(point.x * 1000);
        int y = Mathf.RoundToInt(point.y * 1000);

        return (x * 73856093) ^ (y * 19349663) ^ _worldSeed;
    }

    private Vector2 FindInsideNormal(Edge edge, Vector2 edgeCenter)
    {
        Vector2 edgeVector = edge.End - edge.Start;

        edgeVector.Normalize();

        Vector2 normalA = new(-edgeVector.y, edgeVector.x);
        Vector2 normalB = new(edgeVector.y, -edgeVector.x);

        Vector2 testPointA = edgeCenter + normalA * 0.01f;
        Vector2 testPointB = edgeCenter + normalB * 0.01f;

        if (_cell.ContainsPoint(testPointA))
            return normalA.normalized;
        else if (_cell.ContainsPoint(testPointB))
            return normalB.normalized;

        throw new Exception("Failed to determine inward direction for edge. Check the logic for determining whether points belong to a polygon.");
    }

    private List<Vector2> GenerateRoadPoints(Vector2 start, Vector2 end, List<Vector2> randomPoints)
    {
        List<Edge> edges = new();
        List<Vector2> path = new() { start };

        Vector2 currentPoint = start;

        while (randomPoints.Count > 0)
        {
            bool foundNextPoint = false;

            for (int i = 0; i < randomPoints.Count; i++)
            {
                Vector2 potentialNextPoint = randomPoints[i];
                Edge potentialEdge = new(currentPoint, potentialNextPoint);
                Edge potentialEdgeToEnd = new(potentialNextPoint, end);

                bool intersects = edges.Any(edge => potentialEdge.Intersects(edge.Start, edge.End)) ||
                                  edges.Any(edge => potentialEdgeToEnd.Intersects(edge.Start, edge.End));

                if (!intersects)
                {
                    edges.Add(potentialEdge);
                    path.Add(potentialNextPoint);
                    randomPoints.RemoveAt(i);
                    currentPoint = potentialNextPoint;
                    foundNextPoint = true;
                    break;
                }
            }

            if (!foundNextPoint)
                break;
        }

        path.Add(end);

        return path;
    }
}