using System.Collections.Generic;
using UnityEngine;

namespace Retrover.Math
{
    public class VoronoiCell
    {
        public Vector2 Site { get; private set; }
        public List<Edge> Edges { get; private set; }

        public VoronoiCell(Vector2 site)
        {
            Site = site;
            Edges = new List<Edge>();
        }
    }
}
