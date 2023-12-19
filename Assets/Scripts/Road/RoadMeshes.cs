using Retrover.Path;
using System.Collections.Generic;
using UnityEngine;

public class RoadMeshes : MonoBehaviour
{
    public RoadMesh _roadMeshPrefab;
    private Queue<RoadMesh> _pool = new();

    public RoadMesh GetRoadMesh(List<PathTransform> pathTransforms, float roadWidth)
    {
        RoadMesh roadMesh;
        if (_pool.Count > 0)
            roadMesh = _pool.Dequeue();
        else
            roadMesh = Instantiate(_roadMeshPrefab, transform);

        roadMesh.Activate(pathTransforms, roadWidth);
        return roadMesh;
    }

    public void ReturnRoadMesh(RoadMesh roadMesh)
    {
        roadMesh.Deactivate();
        _pool.Enqueue(roadMesh);
    }
}