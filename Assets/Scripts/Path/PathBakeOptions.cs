using System;
using UnityEngine;

namespace Retrover.Path
{
    [Serializable]
    public class PathBakeOptions
    {
        [field: SerializeField, Range(0, 45)] public float MaximumAngleError { get; private set; } = 0.1f;
        [field: SerializeField, Range(0, 1)] public float MinimumVertexDistance { get; private set; } = 1;
        [field: SerializeField, Range(0, 100)] public int Accuracy { get; private set; } = 10;

        public PathBakeOptions(float maximumAngleError = 0.1f, float minimumVertexDistance = 1f, int accuracy = 10)
        {
            if (maximumAngleError < 0 || maximumAngleError > 45) throw new ArgumentOutOfRangeException("Maximum Angle Error not in range 0..45");
            if (minimumVertexDistance < 0 || minimumVertexDistance > 1) throw new ArgumentOutOfRangeException("Maximum Angle Error not in range 0..1");
            MaximumAngleError = maximumAngleError;
            MinimumVertexDistance = minimumVertexDistance;
            Accuracy = accuracy;
        }
    }
}