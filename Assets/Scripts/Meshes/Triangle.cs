using System.Collections.Generic;
using UnityEngine;

namespace Retrover.Meshes
{
    public struct Triangle : IPolygon
    {
        public Vector3 point1, point2, point3;

        public Triangle(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            point1 = p1;
            point2 = p2;
            point3 = p3;
        }

        public readonly void GenerateForMesh(List<Vector3> vertices, List<int> triangles, List<Vector2> uvs, List<Vector3> normals, ref float uvOffset)
        {
            int startIndex = vertices.Count;

            vertices.AddRange(new[] { point1, point2, point3 });
            triangles.AddRange(new[] { startIndex, startIndex + 1, startIndex + 2 });

            Vector3 normal = Vector3.Cross(point2 - point1, point3 - point1).normalized;
            normals.AddRange(new[] { normal, normal, normal });

            uvs.AddRange(new[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 1) });
        }
    }
}