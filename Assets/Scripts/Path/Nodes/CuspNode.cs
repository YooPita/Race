using UnityEngine;

namespace Retrover.Path
{
    public class CuspNode : INode
    {
        private Vector3 _position;

        public Vector3 Position
        {
            get => _position;
            set
            {
                Vector3 offset = value - _position;
                _position = value;
                Handle1 += offset;
                Handle2 += offset;
            }
        }
        public Vector3 Handle1 { get; set; }
        public Vector3 Handle2 { get; set; }
        public NodeType Type => NodeType.Cusp;

        public CuspNode(Vector3 position, Vector3? handle1 = null, Vector3? handle2 = null)
        {
            _position = position;
            Handle1 = (Vector3)(handle1 == null ? position : handle1);
            Handle2 = (Vector3)(handle2 == null ? position : handle2);
        }
    }
}