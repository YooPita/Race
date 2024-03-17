using System.Collections.Generic;
using UnityEngine;
using Clipper2Lib;
using System.Linq;
using Retrover.Math;

public class ClippingDebug : MonoBehaviour
{
    private List<IPolygon> resultingPolygons = new();

    [SerializeField] private List<Transform> subjectTransforms;
    [SerializeField] private List<Transform> clipTransforms;


    void Start()
    {
        List<Vector2> subjectPoints = new();
        foreach (Transform t in subjectTransforms)
            subjectPoints.Add(new Vector2(t.position.x, t.position.z));
        Polygon subjectPolygon = new(subjectPoints);

        List<Vector2> clipPoints = new();
        foreach (Transform t in clipTransforms)
            clipPoints.Add(new Vector2(t.position.x, t.position.z));
        Polygon clipPolygon = new(clipPoints);

        resultingPolygons = Clip(subjectPolygon, clipPolygon);
    }

    private void DrawPolygon(Polygon polygon, Color color)
    {
        Gizmos.color = color;
        foreach (var edge in polygon.Edges)
        {
            Gizmos.DrawLine(new Vector3(edge.Start.x, 0, edge.Start.y),
                new Vector3(edge.End.x, 0, edge.End.y));
        }
    }

    private void DrawTransforms(List<Transform> transforms, Color color)
    {
        Gizmos.color = color;
        for (int i = 0; i < transforms.Count - 1; i++)
        {
            Gizmos.DrawLine(
                new Vector3(transforms[i].position.x, 0f, transforms[i].position.z),
                new Vector3(transforms[i + 1].position.x, 0f, transforms[i + 1].position.z));
        }
        Gizmos.DrawLine(
                new Vector3(transforms[0].position.x, 0f, transforms[0].position.z),
                new Vector3(transforms[^1].position.x, 0f, transforms[^1].position.z));
    }

    private void OnDrawGizmos()
    {
        if (subjectTransforms.Count > 0)
            DrawTransforms(subjectTransforms, Color.white);

        if (clipTransforms.Count > 0)
            DrawTransforms(clipTransforms, Color.yellow);

        if (resultingPolygons.Count > 0)
            foreach (Polygon polygon in resultingPolygons)
                DrawPolygon(polygon, Color.red);
    }


    private const double ScalingFactor = 10000.0;

    public List<IPolygon> Clip(IPolygon subjectPolygon, IPolygon clipPolygon)
    {
        // Преобразование IPolygon в Paths64 для Clipper
        Paths64 subjectPath = PolygonToPaths64(subjectPolygon);
        Paths64 clipPath = PolygonToPaths64(clipPolygon);

        // Выполнение операции пересечения
        Paths64 solutionPaths = Clipper.Intersect(subjectPath, clipPath, FillRule.NonZero);

        // Преобразование результата обратно в List<IPolygon>
        List<IPolygon> resultPolygons = Paths64ToPolygons(solutionPaths);

        return resultPolygons;
    }

    private Paths64 PolygonToPaths64(IPolygon polygon)
    {
        Paths64 paths = new Paths64();
        Path64 path = new Path64();

        foreach (var edge in polygon.Edges)
        {
            path.Add(new Point64((long)(edge.Start.x * ScalingFactor), (long)(edge.Start.y * ScalingFactor)));
        }

        // Убедимся, что полигон закрыт
        if (polygon.Edges.Count > 0)
        {
            var firstEdge = polygon.Edges[0];
            path.Add(new Point64((long)(firstEdge.Start.x * ScalingFactor), (long)(firstEdge.Start.y * ScalingFactor)));
        }

        paths.Add(path);
        return paths;
    }

    private List<IPolygon> Paths64ToPolygons(Paths64 paths)
    {
        List<IPolygon> polygons = new List<IPolygon>();

        foreach (var path in paths)
        {
            List<Vector2> points = new List<Vector2>();

            foreach (var point in path)
            {
                points.Add(new Vector2((float)point.X / (float)ScalingFactor, (float)point.Y / (float)ScalingFactor));
            }

            // Удаляем дублирующую первую точку в конце, если она есть
            if (points.Count > 1 && points[0] == points[points.Count - 1])
            {
                points.RemoveAt(points.Count - 1);
            }

            polygons.Add(new Polygon(points));
        }

        return polygons;
    }
}
