using System.Collections.Generic;
using UnityEngine;

namespace Retrover.Math
{
    public interface IPointsGroup : IEnumerable<Vector2>
    {
        void RemoveDuplicates();
        Triangle SuperTriangle();
    }
}
