using Retrover.Math;
using Retrover.Path;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ChunkDebug : MonoBehaviour
{
    [SerializeField] private Transform _starRoadTransform;
    [SerializeField] private Transform _endRoadTransform;
    [SerializeField] private float _roadWidth = 1.0f;
    [SerializeField] private RoadMeshesPool _roadMeshes;
    [SerializeField] private Transform _roadStart;
    [SerializeField] private Transform _roadStartHandle;
    [SerializeField] private Transform _roadMidle;
    [SerializeField] private Transform _roadMidleHandle;
    [SerializeField] private Transform _roadEnd;
    [SerializeField] private Transform _roadEndHandle;
    [SerializeField] private PathBakeOptions _bakeOptions;
    [SerializeField] private float _houseDistance = 3f;

    private readonly List<Vector2> _voronoiPoints = new();
    private readonly List<VoronoiCell> _voronoiCells = new();

    private void Start()
    {
        Generate();
    }

    private void Generate()
    {
        IPath path = Path();

        for (int i = 0; i < path.SegmentsCount; i++)
        {
            ISegment segment = path.SegmentAtIndex(i);
            RoadMeshNew mesh = _roadMeshes.RoadMesh(segment.TransformsCopy(), _roadWidth);
        }

        GenerateVoronoiPoints(path);
        GenerateVoronoi();
    }

    private IPath Path()
    {
        IPath path = new Path(_bakeOptions);
        path.AddNode(new SymmetricNode(_roadStart.position, _roadStartHandle.position));
        path.AddNode(new SymmetricNode(_roadMidle.position, _roadMidleHandle.position));
        path.AddNode(new SymmetricNode(_roadEnd.position, _roadEndHandle.position));
        path.Bake();
        return path;
    }

    private void GenerateVoronoiPoints(IPath path)
    {
        List<Vector2> points = new();
        float sideDistance = _houseDistance / 2f;

        for (float i = sideDistance; i < path.Length; i += _houseDistance)
        {
            PathTransform pathTransform = path.Transform(i);
            Vector3 normal = pathTransform.Normal;
            Vector3 position = pathTransform.Position;

            Vector2 position2D = new(position.x, position.z);

            Vector2 leftNormal = new(-normal.z, normal.x);
            Vector2 rightNormal = new(normal.z, -normal.x);

            Vector2 leftPoint = position2D + leftNormal * sideDistance;
            Vector2 rightPoint = position2D + rightNormal * sideDistance;

            points.Add(leftPoint);
            points.Add(rightPoint);
            points.Add(position2D);
        }

        //_voronoiPoints.AddRange(FilterVoronoiPoints(points));
        _voronoiPoints.AddRange(points);
    }

    private List<Vector2> FilterVoronoiPoints(List<Vector2> points)
    {
        List<Vector2> filteredPoints = new List<Vector2>();
        float minimalDistance = _houseDistance / 2f;

        // Проходим по каждой точке в списке
        foreach (var point in points)
        {
            bool isIsolated = true;

            // Сравниваем с каждой другой точкой
            foreach (var otherPoint in points)
            {
                if (point == otherPoint)
                    continue; // Пропускаем, если сравниваем точку саму с собой

                float distance = Vector2.Distance(point, otherPoint);

                // Если расстояние меньше или равно _houseDistance, точка не изолирована
                if (distance < minimalDistance)
                {
                    isIsolated = false;
                    break; // Выходим из внутреннего цикла, так как точка не удовлетворяет условию
                }
            }

            // Если точка изолирована, добавляем её в фильтрованный список
            if (isIsolated)
            {
                filteredPoints.Add(point);
            }
        }

        return filteredPoints;
    }

    private void GenerateVoronoi()
    {
        PointsGroup points = new(_voronoiPoints);
        VoronoiDiagram voronoi = new(points);
        voronoi.Calculate();
        _voronoiCells.AddRange(voronoi.Cells);
    }

    private void OnDrawGizmos()
    {
        if (_voronoiPoints.Count > 0)
        {
            Gizmos.color = Color.gray;
            for (int i = 0; i < _voronoiPoints.Count; i++)
            {
                Gizmos.DrawSphere(new Vector3(_voronoiPoints[i].x, -0.2f, _voronoiPoints[i].y), 0.1f);
            }
        }

        if (_voronoiCells.Count > 0)
        {
            Gizmos.color = Color.gray;
            foreach (VoronoiCell cell in _voronoiCells)
            {
                for (int i = 0; i < cell.Edges.Count; i++)
                {
                    Vector3 point1 = new(cell.Edges[i].Start.x, -0.2f, cell.Edges[i].Start.y);
                    Vector3 point2 = new(cell.Edges[i].End.x, -0.2f, cell.Edges[i].End.y);
                    Gizmos.DrawLine(point1, point2);
                }
            }
        }
    }
}
