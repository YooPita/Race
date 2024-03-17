using Retrover.Path;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadDebug : MonoBehaviour
{
    [SerializeField] private PathDebug _pathDebug;
    [SerializeField] private RoadMeshesPool _roadMeshes;
    [SerializeField] private float _roadWidth = 15f;
    private List<RoadMeshNew> _roadMeshesList = new();
    private IPath _path;

    [ContextMenu("Regenerate")]
    private void Regenerate()
    {
        CreateRoad();
    }

    private void CreateRoad()
    {
        ClearRoad(_roadMeshesList, _roadMeshes);
        _path = _pathDebug.Path();

        for (int i = 0; i < _path.SegmentsCount; i++)
        {
            ISegment segment = _path.SegmentAtIndex(i);
            RoadMeshNew mesh = _roadMeshes.RoadMesh(segment.TransformsCopy(), _roadWidth);
            _roadMeshesList.Add(mesh);
        }
    }

    private void ClearRoad(List<RoadMeshNew> roadMeshesList, RoadMeshesPool roadMeshes)
    {
        if (roadMeshesList.Count == 0)
            return;

        foreach (var mesh in roadMeshesList)
            roadMeshes.Push(mesh);

        roadMeshesList.Clear();
    }
}
