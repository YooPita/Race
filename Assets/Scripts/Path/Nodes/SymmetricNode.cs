using UnityEngine;

namespace Retrover.Path
{
    public class SymmetricNode : INode
    {
        private Vector3 _position;
        private Vector3 _handle1;
        private Vector3 _handle2;

        public Vector3 Position
        {
            get => _position;
            set
            {
                Vector3 offset = value - _position;
                _position = value;
                _handle1 += offset;
                _handle2 += offset;
            }
        }

        public Vector3 Handle1
        {
            get => _handle1;
            set
            {
                _handle1 = value;
                UpdateHandle(ref _handle2, _handle1);
            }
        }

        public Vector3 Handle2
        {
            get => _handle2;
            set
            {
                _handle2 = value;
                UpdateHandle(ref _handle1, _handle2);
            }
        }

        public NodeType Type => NodeType.Symmetric;

        public SymmetricNode(Vector3 position, Vector3 handle)
        {
            _position = position;
            _handle1 = handle;
            _handle2 = handle;
            UpdateHandle(ref _handle2, _handle1);
        }

        private void UpdateHandle(ref Vector3 handle, Vector3 byHandle)
        {
            Vector3 oppositePoint = _position - (byHandle - _position);
            float distance = (byHandle - _position).magnitude;
            handle = _position + (oppositePoint - _position).normalized * distance;
        }
    }

}