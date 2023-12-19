using System.Collections.Generic;
using UnityEngine;

namespace Retrover.Path
{
    public class BezierSegment : ISegmnetBaker
    {
        public Vector3 StartPoint { get; private set; }
        public Vector3 EndPoint { get; private set; }

        private Vector3 _startHandle;
        private Vector3 _endHandle;

        public BezierSegment(Vector3 startPoint, Vector3 endPoint, Vector3 startHandle, Vector3 endHandle)
        {
            StartPoint = startPoint;
            EndPoint = endPoint;
            _startHandle = startHandle;
            _endHandle = endHandle;
        }

        public ISegment Bake(PathBakeOptions options)
        {
            float length = CalculateLength();
            float step = 1f / Mathf.CeilToInt(length * options.Accuracy);
            List<Vector3> points = new();
            float errorDot = 1f - options.MaximumAngleError / 90f;
            float distanceLastVertex = 0;
            points.Add(StartPoint);
            Vector3 findedPoint = CurvePoint(step);
            for (float i = step; i <= 1f - step; i += step)
            {
                Vector3 findedNextPoint = CurvePoint(i + step);
                var dot = Vector3.Dot((findedPoint - points[^1]).normalized, (findedNextPoint - findedPoint).normalized);
                if (dot <= errorDot && distanceLastVertex >= options.MinimumVertexDistance)
                {
                    points.Add(findedPoint);
                    distanceLastVertex = 0;
                }
                else
                    distanceLastVertex += Vector3.Distance(points[^1], findedPoint);
                findedPoint = findedNextPoint;
            }
            points.Add(EndPoint);

            List<PathTransform> transforms = new();
            transforms.Add(new(StartPoint, (_startHandle - StartPoint).normalized));

            for (int i = 1; i < points.Count - 1; i++)
                transforms.Add(new(points[i], (points[i + 1] - points[i]).normalized));

            transforms.Add(new(EndPoint, (EndPoint - _endHandle).normalized));

            BackedSegment backedSegment = new(transforms);
            backedSegment.Bake();
            return backedSegment;
        }

        private float CalculateLength()
        {
            float length = (StartPoint - _startHandle).magnitude +
                (_startHandle - _endHandle).magnitude +
                (_endHandle - EndPoint).magnitude;
            return (StartPoint - EndPoint).magnitude + length / 2f;
        }

        private Vector3 CurvePoint(float lerp)
        {
            var part1 = Vector3.Lerp(StartPoint, _startHandle, lerp);
            var part2 = Vector3.Lerp(_startHandle, _endHandle, lerp);
            var part3 = Vector3.Lerp(_endHandle, EndPoint, lerp);

            return Vector3.Lerp(
                Vector3.Lerp(part1, part2, lerp),
                Vector3.Lerp(part2, part3, lerp),
                lerp);
        }
    }
}