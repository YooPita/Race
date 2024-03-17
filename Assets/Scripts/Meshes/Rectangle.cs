using System.Collections.Generic;
using UnityEngine;

namespace Retrover.Meshes
{
    public struct Rectangle : IPolygon
    {
        public Vector3 topLeft, topRight, bottomLeft, bottomRight;

        public Rectangle(Vector3 tl, Vector3 tr, Vector3 bl, Vector3 br)
        {
            topLeft = tl;
            topRight = tr;
            bottomLeft = bl;
            bottomRight = br;
        }

        public readonly void GenerateForMesh(List<Vector3> vertices, List<int> triangles, List<Vector2> uvs, List<Vector3> normals, ref float uvOffset)
        {
            int startIndex = vertices.Count;

            vertices.AddRange(new[] { topLeft, bottomLeft, topRight, bottomRight });
            triangles.AddRange(new[] { startIndex, startIndex + 2, startIndex + 1, startIndex + 1, startIndex + 2, startIndex + 3 });

            Vector3 normal = Vector3.Cross(topRight - topLeft, bottomLeft - topLeft).normalized;
            normals.AddRange(new[] { normal, normal, normal, normal });

            //uvs.AddRange(new[] { new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0), new Vector2(0, 0) });

            float distanceLeft = Vector3.Distance(topLeft, topRight);
            float distanceRight = Vector3.Distance(bottomLeft, bottomRight);
            float distance = Mathf.Max(distanceLeft, distanceRight) * 0.07f;

            float uvDistance = uvOffset + distance;

            uvs.AddRange(new[]{
                new Vector2(uvOffset, 0),
                new Vector2(uvOffset, 1),
                new Vector2(uvDistance, 0),
                new Vector2(uvDistance, 1)});

            uvOffset += distance;
        }
    }
}