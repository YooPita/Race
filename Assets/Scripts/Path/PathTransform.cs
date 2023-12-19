using UnityEngine;

namespace Retrover.Path
{
    public struct PathTransform
    {
        public Vector3 Position { get; private set; }
        public Vector3 Normal { get; private set; }

        public PathTransform(Vector3 position, Vector3 normal)
        {
            Position = position;
            Normal = normal;
        }
    }
}