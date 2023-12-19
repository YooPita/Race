using System.Collections.Generic;
using UnityEngine;

namespace Retrover.Path
{
    public interface ISegment
    {
        Vector3 StartPoint { get; }
        Vector3 EndPoint { get; }
        float Length { get; }
        PathTransform TransformAt(float length);
        List<PathTransform> TransformsCopy();
    }
}