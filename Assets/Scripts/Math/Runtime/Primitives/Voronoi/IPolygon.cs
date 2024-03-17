using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace Retrover.Math
{
    public interface IPolygon
    {
        ReadOnlyCollection<Edge> Edges { get; }

        bool ContainsPoint(Vector2 point);

        protected static bool PointInPolygon(Vector2 point, IPolygon polygon)
        {
            bool isInside = false;

            for (int i = 0; i < polygon.Edges.Count; i++)
            {
                Vector2 start = polygon.Edges[i].Start;
                Vector2 end = polygon.Edges[i].End;

                if ((start.y > point.y) != (end.y > point.y))
                {
                    float intersectX = (point.y - start.y) * (end.x - start.x) / (end.y - start.y) + start.x;

                    if (point.x < intersectX)
                    {
                        isInside = !isInside;
                    }
                }
            }

            return isInside;
        }
    }
}