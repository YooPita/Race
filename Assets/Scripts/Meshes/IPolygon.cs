using System.Collections.Generic;
using UnityEngine;

namespace Retrover.Meshes
{
    public interface IPolygon
    {
        void GenerateForMesh(List<Vector3> vertices, List<int> triangles, List<Vector2> uvs, List<Vector3> normals, ref float uvOffset);
    }
}