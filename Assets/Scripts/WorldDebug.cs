using Retrover.Math;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class WorldDebug : MonoBehaviour
{
    [Inject] private readonly WorldStream _stream;

    private void OnDrawGizmos()
    {
        if (_stream == null)
            return;

        List<VoronoiCell> cornerCells = new();

        foreach (VoronoiCell cell in _stream.Cells)
        {
            foreach (var edgeWithNeighbor in cell.EdgesWithNeighbors)
            {
                if (edgeWithNeighbor.neighbor == null)
                {
                    cornerCells.Add(cell);
                    break;
                }
            }
        }

        foreach (VoronoiCell cell in _stream.Cells)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(new Vector3(cell.Site.x, 0, cell.Site.y), 1f);
            Gizmos.color = Color.white;
            foreach (var edge in cell.Edges)
            {
                Gizmos.DrawLine(
                    new Vector3(edge.Start.x, 0, edge.Start.y),
                    new Vector3(edge.End.x, 0, edge.End.y));
            }
        }

        if(_stream.CurrentCell != null)
        {
            Gizmos.color = Color.blue;
            foreach (var edge in _stream.CurrentCell.Edges)
            {
                Gizmos.DrawLine(
                    new Vector3(edge.Start.x, 0, edge.Start.y),
                    new Vector3(edge.End.x, 0, edge.End.y));
            }
        }
    }
}
