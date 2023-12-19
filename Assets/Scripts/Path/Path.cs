using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Retrover.Path
{
    public class Path : IPath
    {
        public float Length => _length;

        public int NodesCount => _nodes.Count;

        public int SegmentsCount => _segments.Count;

        private List<INode> _nodes = new();
        private List<ISegment> _segments = new();
        private List<float> _segmentLengths = new();
        private bool isDirty = true;
        private float _length = 0f;
        private readonly PathBakeOptions _bakeOptions;

        public Path(PathBakeOptions bakeOptions)
        {
            _bakeOptions = bakeOptions;
        }

        public void AddNode(INode node)
        {
            _nodes.Add(node);
            if (_nodes.Count > 1)
                isDirty = true;
        }

        public void Bake()
        {
            if (!isDirty) return;

            _segments.Clear();
            _segmentLengths.Clear();
            _length = 0;

            for (int i = 0; i < _nodes.Count - 1; i++)
            {
                ISegmnetBaker baker = CreateCurveBetween(_nodes[i], _nodes[i + 1]);
                ISegment segment = baker.Bake(_bakeOptions);
                _segments.Add(segment);

                float segmentLength = segment.Length;
                _segmentLengths.Add(segmentLength);
                _length += segmentLength;
            }

            isDirty = false;
        }

        private ISegmnetBaker CreateCurveBetween(INode node1, INode node2)
        {
            return new BezierSegment(node1.Position, node2.Position, node1.Handle1, node2.Handle2);
        }

        public PathTransform Transform(float length)
        {
            if (length < 0 || length > _length)
                throw new ArgumentOutOfRangeException("length");

            float currentLength = 0;
            for (int i = 0; i < _segments.Count; i++)
            {
                currentLength += _segmentLengths[i];
                if (length <= currentLength)
                {
                    float segmentRelativeLength = length - (currentLength - _segmentLengths[i]);
                    return _segments[i].TransformAt(segmentRelativeLength);
                }
            }

            throw new Exception("Length is out of path range");
        }

        public void RemoveNode(INode node)
        {
            if (_nodes.Contains(node))
            {
                _nodes.Remove(node);
                isDirty = true;
            }
            else
            {
                throw new ArgumentException("The node does not exist in the path");
            }
        }

        public void RemoveNodeAtIndex(int index)
        {
            if (index >= 0 && index < _nodes.Count)
            {
                _nodes.RemoveAt(index);
                isDirty = true;
            }
            else
            {
                throw new ArgumentOutOfRangeException("index", "Index is out of range");
            }
        }

        public List<INode> NodesCopy()
        {
            return new List<INode>(_nodes);
        }

        public INode NodeAtIndex(int index)
        {
            if (index < 0 || index >= _nodes.Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            return _nodes[index];
        }

        public ISegment SegmentAtIndex(int index)
        {
            if (index < 0 || index >= _segments.Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            return _segments[index];
        }
    }
}