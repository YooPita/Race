using UnityEngine;
using UnityEngine.UIElements;

namespace Retrover.Path
{
    public class SmoothNode : INode
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

        public NodeType Type => NodeType.Smooth;

        public SmoothNode(Vector3 position, Vector3 handle1, Vector3? handle2 = null)
        {
            _position = position;
            _handle1 = handle1;
            _handle2 = (Vector3)(handle2 == null ? handle1 : handle2);
            UpdateHandle(ref _handle2, _handle1);
        }

        private void UpdateHandle(ref Vector3 handle, Vector3 byHandle)
        {
            Vector3 oppositePoint = _position - (byHandle - _position);
            float distance = (handle - _position).magnitude;
            handle = _position + (oppositePoint - _position).normalized * distance;
        }
    }
}