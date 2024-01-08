using Retrover.ObjectPool;
using Retrover.Path;
using System.Collections.Generic;

public class RoadMeshes : MonoPool<RoadMesh>
{
    public RoadMesh RoadMesh(List<PathTransform> pathTransforms, float roadWidth)
    {
        RoadMesh roadMesh = Pull();
        roadMesh.Initialize(pathTransforms, roadWidth);
        return roadMesh;
    }
}