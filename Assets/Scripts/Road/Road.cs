using Retrover.Path;
using System.Collections.Generic;
using UnityEngine;

public class Road
{
    public float StartLength { get; private set; }
    public float EndLength => _path.Length + StartLength;

    public List<PathTransform> Transforms => _transforms;

    [SerializeField] private float _segmentLength = 35f; // Примерная длина одного отрезка дороги
    [SerializeField] private float _maxTurnAngle = 30f; // Максимальный угол поворота
    [SerializeField] private float _maxGrade = 10f; // Максимальный угол подъема/спуска
    [SerializeField] private int _maxSegments = 10; // Максимальный угол подъема/спуска
    [SerializeField] private int _renderDistance = 5; // Максимальный угол подъема/спуска
    [SerializeField] private PathBakeOptions _bakeOptions;
    private Vector3 _currentEndPoint;
    private int _nodesCount = 0;
    private IPath _path;
    private List<PathTransform> _transforms = new();
    private RoadMeshesPool _roadMeshes;
    private List<RoadMeshNew> _roadMeshesList = new();
    private float _roadWidth = 15f;

    public Road(Vector3 startPosition, PathBakeOptions bakeOptions, RoadMeshesPool roadMeshes)
    {
        _bakeOptions = bakeOptions;
        _path = new Path(_bakeOptions);
        StartLength = 0f;
        _currentEndPoint = startPosition;
        _roadMeshes = roadMeshes;
        GenerateInitialRoad();
    }

    public PathTransform Position(float position)
    {
        if (position < StartLength || position > EndLength)
            throw new System.ArgumentOutOfRangeException("position");

        return _path.Transform(position - StartLength);
    }

    private void GenerateInitialRoad()
    {
        _path.AddNode(new SymmetricNode(_currentEndPoint, _currentEndPoint + Vector3.forward * _segmentLength / 2f));
        _nodesCount++;
        for (int i = 0; i < _maxSegments; i++) // Начальная генерация дороги
            AddRoadSegment();
    }

    public void UpdateRoad(float userPosition)
    {
        if (userPosition < StartLength || userPosition > EndLength)
            throw new System.ArgumentOutOfRangeException("userPosition");

        if (NeedsNewSegment(userPosition))
        {
            AddRoadSegment();
            RemoveFirstSegment();
        }
    }

    private void AddRoadSegment()
    {
        Vector3 direction = RandomDirection();
        Vector3 newEndPoint = _currentEndPoint + direction * _segmentLength;
        Vector3 handle = newEndPoint + direction * _segmentLength / 2f;
        SymmetricNode newNode = new(newEndPoint, handle);
        _path.AddNode(newNode);

        _currentEndPoint = newEndPoint;
        _nodesCount++;
        _path.Bake();

        ISegment segment = _path.SegmentAtIndex(_path.SegmentsCount - 1);
        _roadMeshesList.Add(_roadMeshes.RoadMesh(segment.TransformsCopy(), _roadWidth));

        BakeDebugPoints();
    }

    private Vector3 RandomDirection()
    {
        float turnAngle = Random.Range(-_maxTurnAngle, _maxTurnAngle);
        float grade = Random.Range(-_maxGrade, _maxGrade);
        Quaternion rotation = Quaternion.Euler(grade, turnAngle, 0);
        return rotation * Vector3.forward;
    }

    private bool NeedsNewSegment(float userPosition)
    {
        int currentSegment;
        float currentLength = StartLength;

        for (currentSegment = 0; currentSegment < _path.SegmentsCount; currentSegment++)
        {
            currentLength += _path.SegmentAtIndex(currentSegment).Length;
            if (userPosition < currentLength)
                break;
        }

        return currentSegment >= _maxSegments - _renderDistance;
    }

    private void RemoveFirstSegment()
    {
        _roadMeshes.Push(_roadMeshesList[0]);
        _roadMeshesList.RemoveAt(0);
        StartLength += _path.SegmentAtIndex(0).Length;
        _path.RemoveNodeAtIndex(0);
        _nodesCount--;
        _path.Bake();
        BakeDebugPoints();
    }

    private void BakeDebugPoints()
    {
        _transforms.Clear();
        for (int i = 0; i < _path.SegmentsCount; i++)
        {
            List<PathTransform> segmentTransforms = _path.SegmentAtIndex(i).TransformsCopy();

            // Для всех сегментов, кроме первого, удаляем первую точку
            if (i > 0 && segmentTransforms.Count > 0)
            {
                segmentTransforms.RemoveAt(0);
            }

            _transforms.AddRange(segmentTransforms);
        }
    }
}
