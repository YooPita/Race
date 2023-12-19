using System;
using System.Collections.Generic;
using UnityEngine;

namespace Retrover.Path
{
    public class BackedSegment : ISegment
    {
        public Vector3 StartPoint => _transforms[0].Position;

        public Vector3 EndPoint => _transforms[^1].Position;

        public float Length => _length;

        private float _length = -1;
        private List<PathTransform> _transforms = new();
        private List<float> _linesLength = new();

        public BackedSegment(List<PathTransform> transforms)
        {
            if (transforms == null || transforms.Count == 0)
                throw new ArgumentException();

            _transforms.AddRange(transforms);
        }

        public PathTransform TransformAt(float length)
        {
            if (_length == -1)
                throw new InvalidOperationException("Bake method must be called before TransformAt.");

            if (length < 0 || length > Length)
                throw new ArgumentException();

            if (length < Mathf.Epsilon)
                return _transforms[0];

            if (length > Length - Mathf.Epsilon)
                return _transforms[^1];

            float accumulatedLength = 0;
            for (int i = 0; i < _linesLength.Count; i++)
            {
                accumulatedLength += _linesLength[i];
                if (length <= accumulatedLength)
                {
                    float lerp = (length - (accumulatedLength - _linesLength[i])) / _linesLength[i];
                    PathTransform startTransform = _transforms[i];
                    PathTransform endTransform = _transforms[i + 1];

                    Vector3 position = Vector3.Lerp(startTransform.Position, endTransform.Position, lerp);
                    Vector3 normal = Vector3.Lerp(startTransform.Normal, endTransform.Normal, lerp);
                    return new PathTransform(position, normal);
                }
            }

            throw new InvalidOperationException("Length is out of segment bounds.");
        }

        public void Bake()
        {
            _length = 0;
            for (int i = 1; i < _transforms.Count; i++)
            {
                float currentLength = Vector3.Distance(_transforms[i - 1].Position, _transforms[i].Position);
                _length += currentLength;
                _linesLength.Add(currentLength);
            }
        }

        public List<PathTransform> TransformsCopy()
        {
            return new List<PathTransform>(_transforms);
        }
    }
}