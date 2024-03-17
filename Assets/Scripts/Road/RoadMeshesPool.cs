using Retrover.ObjectPool;
using Retrover.Path;
using System.Collections.Generic;

public class RoadMeshesPool : MonoPool<RoadMeshNew>
{
    public RoadMeshNew RoadMesh(List<PathTransform> pathTransforms, float roadWidth)
    {
        RoadMeshNew roadMesh = Pull();
        roadMesh.Initialize(pathTransforms, roadWidth);
        return roadMesh;
    }
}