using Retrover.Path;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class RoadMesh : MonoBehaviour
{
    private Mesh _mesh;
    private float _roadWidth = 1.0f;
    private List<PathTransform> _pathTransforms = new();
    private MeshFilter _filter;
    private MeshRenderer _renderer;

    void Awake()
    {
        _filter = GetComponent<MeshFilter>();
        _renderer = GetComponent<MeshRenderer>();
    }

    public void Activate(List<PathTransform> pathTransforms, float width)
    {
        _roadWidth = width;
        _pathTransforms.AddRange(pathTransforms);
        GenerateMesh();
    }

    public void Deactivate()
    {
        _mesh?.Clear();
        _pathTransforms.Clear();
    }

    private void GenerateMesh()
    {
        _mesh = new Mesh();
        List<Vector3> vertices = new();
        List<int> triangles = new();
        List<Vector3> normals = new(); // Добавление списка для нормалей
        List<Vector2> uvs = new();
        List<KeyValuePair<Vector3, Vector3>> lines = new();
        float uvOffset = 0f;

        for (int i = 0; i < _pathTransforms.Count; i++)
        {
            Vector3 leftDirCurrent = new Vector3(-_pathTransforms[i].Normal.z, 0, _pathTransforms[i].Normal.x);
            lines.Add(new KeyValuePair<Vector3, Vector3>(
                _pathTransforms[i].Position + leftDirCurrent * _roadWidth / 2,
                _pathTransforms[i].Position - leftDirCurrent * _roadWidth / 2));
        }

        // Корректировка пересекающихся линий
        //CorrectIntersectingLines3(ref lines);

        for (int i = 0; i < lines.Count - 1; i++)
        {
            vertices.Add(lines[i].Key);
            vertices.Add(lines[i].Value);
            vertices.Add(lines[i + 1].Key);
            vertices.Add(lines[i + 1].Value);

            // Добавление UV-координат
            float distanceLeft = Vector3.Distance(lines[i].Key, lines[i + 1].Key);
            float distanceRight = Vector3.Distance(lines[i].Value, lines[i + 1].Value);

            float distance = Mathf.Max(distanceLeft, distanceRight) * 0.07f;

            float uvDistance = uvOffset + distance;
            uvs.AddRange(new List<Vector2>
            {
                new Vector2(uvOffset, 0),
                new Vector2(uvOffset, 1),
                new Vector2(uvDistance, 0),
                new Vector2(uvDistance, 1)
            });
            uvOffset += distance;

            normals.Add(Vector3.up);
            normals.Add(Vector3.up);
            normals.Add(Vector3.up);
            normals.Add(Vector3.up);

            int startIndex = i * 4;
            triangles.Add(startIndex);
            triangles.Add(startIndex + 2);
            triangles.Add(startIndex + 1);

            triangles.Add(startIndex + 1);
            triangles.Add(startIndex + 2);
            triangles.Add(startIndex + 3);
        }

        _mesh.SetVertices(vertices);
        _mesh.SetTriangles(triangles, 0);
        //_mesh.SetNormals(normals); // Установка нормалей
        _mesh.SetUVs(0, uvs);
        _mesh.RecalculateNormals();
        _filter.mesh = _mesh;
    }

    private Vector3 GetMiddlePoint(Vector3[] points)
    {
        Vector3 middlePoint = Vector3.zero;
        foreach (var point in points)
            middlePoint += point;
        middlePoint /= points.Length;

        Vector3 closestPoint = points
            .Where(point => Vector3.Distance(point, middlePoint) >= _roadWidth / 2)
            .OrderBy(point => Vector3.Distance(point, middlePoint))
            .FirstOrDefault();

        return closestPoint != Vector3.zero ? closestPoint : middlePoint;
    }

    private List<KeyValuePair<Vector3, Vector3>> FixPoints(ref List<KeyValuePair<Vector3, Vector3>> lines, int startIndex, int endIndex)
    {
        Vector3 middlePoint = GetMiddlePoint(lines.Select(line => line.Key).ToArray());

        int middlePointIndex = startIndex + (endIndex - startIndex) / 2;

        Vector3 offset = middlePoint - lines[middlePointIndex].Key;
        for (int i = startIndex; i <= endIndex; i++)
            lines[i] = new KeyValuePair<Vector3, Vector3>(lines[i].Key + offset, lines[i].Value + offset);

        float scaleFactor = (_roadWidth / 2) / Vector3.Distance(_pathTransforms[middlePointIndex].Position, middlePoint);
        List<KeyValuePair<Vector3, Vector3>> result = new();
        for (int i = startIndex; i <= endIndex; i++)
            result.Add(new(
                _pathTransforms[i].Position + scaleFactor * (lines[i].Key - _pathTransforms[i].Position),
                _pathTransforms[i].Position + scaleFactor * (lines[i].Value - _pathTransforms[i].Position)
            ));

        return result;
    }

    static float CalculateTotalLength(List<Vector3> points)
    {
        float totalLength = 0f;

        for (int i = 0; i < points.Count - 1; i++)
            totalLength += Vector3.Distance(points[i], points[i + 1]);

        return totalLength;
    }

    private bool IsIntersecting(KeyValuePair<Vector3, Vector3> line1, KeyValuePair<Vector3, Vector3> line2)
    {
        // Вычисление направления векторов
        Vector3 dir1 = line1.Value - line1.Key;
        Vector3 dir2 = line2.Value - line2.Key;

        // Перекрестное произведение векторов
        float cross = dir1.x * dir2.z - dir1.z * dir2.x;

        // Проверка, параллельны ли линии
        if (Mathf.Abs(cross) < Mathf.Epsilon)
            return false;

        Vector3 distance = line2.Key - line1.Key;
        float t1 = (distance.x * dir2.z - distance.z * dir2.x) / cross;
        float t2 = (distance.x * dir1.z - distance.z * dir1.x) / cross;

        // Проверка, находится ли пересечение в пределах отрезков
        return t1 >= 0 && t1 <= 1 && t2 >= 0 && t2 <= 1;
    }

    private Vector3 GetFrontPoint(Vector3 line1Start, Vector3 line1End, Vector3 line2Start, Vector3 line2End, Vector3 direction)
    {
        // Вектор нормали к первому отрезку
        Vector3 normal = Vector3.Cross(direction, Vector3.up);

        // Скалярные произведения нормали с векторами к точкам второго отрезка
        float dotStart = Vector3.Dot(normal, line2Start - line1Start);
        float dotEnd = Vector3.Dot(normal, line2End - line1Start);

        // Определяем, какая точка находится спереди
        return dotStart > dotEnd ? line2Start : line2End;
    }

    private Vector3 GetDirection(Vector3 start, Vector3 end)
    {
        return (end - start).normalized;
    }

    private void CorrectIntersectingLines2(ref List<KeyValuePair<Vector3, Vector3>> lines)
    {
        for (int i = 1; i < lines.Count - 1; i++)
            for (int j = i; j < lines.Count - 1; j++)
                MergeIntersectingPoints(ref lines, j - 1, j);

        MergeIntersectingPoints(ref lines, lines.Count - 1, lines.Count - 2);
    }

    private void MergeIntersectingPoints(ref List<KeyValuePair<Vector3, Vector3>> lines, int point1, int point2)
    {
        Vector2? intersectingPoint = ComputeIntersectionPoint(lines[point1], lines[point2]);
        
        if (intersectingPoint != null)
        {
            float distance1 = Vector2.Distance((Vector2)intersectingPoint, lines[point2].Key);
            float distance2 = Vector2.Distance((Vector2)intersectingPoint, lines[point2].Value);
            if (distance1 < distance2)
                lines[point2] = new(lines[point1].Key, lines[point2].Value);
            else
                lines[point2] = new(lines[point2].Key, lines[point1].Value);
        }
    }

    private Vector2? ComputeIntersectionPoint(KeyValuePair<Vector3, Vector3> line1, KeyValuePair<Vector3, Vector3> line2)
    {
        KeyValuePair<Vector2, Vector2> line1Flat = new(
                new Vector3(line1.Key.x, line1.Key.z),
                new Vector3(line1.Value.x, line1.Value.z));

        KeyValuePair<Vector2, Vector2> line2Flat = new(
            new Vector3(line2.Key.x, line2.Key.z),
            new Vector3(line2.Value.x, line2.Value.z));
        return ComputeIntersectionPoint(line1Flat, line2Flat);
    }

    private Vector2? ComputeIntersectionPoint(KeyValuePair<Vector2, Vector2> line1, KeyValuePair<Vector2, Vector2> line2)
    {
        float d = (line1.Key.x - line1.Value.x) * (line2.Key.y - line2.Value.y) -
                  (line1.Key.y - line1.Value.y) * (line2.Key.x - line2.Value.x);

        if (!Mathf.Approximately(d, 0f))
        {
            float t = ((line2.Key.x - line1.Key.x) * (line2.Key.y - line2.Value.y) -
                       (line2.Key.y - line1.Key.y) * (line2.Key.x - line2.Value.x)) / d;

            float u = -((line1.Key.x - line1.Value.x) * (line2.Key.y - line1.Key.y) -
                        (line1.Key.y - line1.Value.y) * (line2.Key.x - line1.Key.x)) / d;

            if (t >= 0f && t <= 1f && u >= 0f && u <= 1f)
                return new Vector2(line1.Key.x + t * (line1.Value.x - line1.Key.x),
                                                        line1.Key.y + t * (line1.Value.y - line1.Key.y));
        }

        return null;
    }

    private void CorrectIntersectingLines(ref List<KeyValuePair<Vector3, Vector3>> lines)
    {
        float minimalDistance = 1f;

        for (int i = 0; i < lines.Count - 2; i++)
        {
            float distanceToLeft = Vector2.Distance(
                new Vector2(lines[i].Key.x, lines[i].Key.z), 
                new Vector2(lines[i + 1].Key.x, lines[i + 1].Key.z));
            float distanceToRight = Vector2.Distance(
                new Vector2(lines[i].Value.x, lines[i].Value.z),
                new Vector2(lines[i + 1].Value.x, lines[i + 1].Value.z));

            if (distanceToLeft < minimalDistance)
                lines[i + 1] = new(lines[i].Key, lines[i + 1].Value);

            if (distanceToRight < minimalDistance)
                lines[i + 1] = new(lines[i + 1].Key, lines[i].Value);

            if (IsIntersecting(lines[i], lines[i + 1]))
            {
                Vector3 frontPoint = GetFrontPoint(lines[i].Key, lines[i].Value, lines[i + 1].Key, lines[i + 1].Value, _pathTransforms[i].Normal.normalized);
                if (frontPoint == lines[i + 1].Key)
                    lines[i + 1] = new(lines[i + 1].Key, lines[i].Value);
                else
                    lines[i + 1] = new(lines[i].Key, lines[i + 1].Value);
            }

            //for (int j = i + 1; j < lines.Count; j++)
            //{
            //    if (IsIntersecting(lines[i].Key, lines[i].Value, lines[j].Key, lines[j].Value))
            //    {
            //        float distanceToLeft = Vector3.Distance(lines[i].Key, lines[j].Key);
            //        float distanceToRight = Vector3.Distance(lines[i].Value, lines[j].Value);

            //        if (distanceToLeft <= 0.5f)
            //        {
            //            // Установка позиции следующей левой точки равной текущей
            //            lines[j] = new KeyValuePair<Vector3, Vector3>(lines[i].Key, lines[j].Value);
            //        }
            //        if (distanceToRight <= 0.5f)
            //        {
            //            // Установка позиции следующей правой точки равной текущей
            //            lines[j] = new KeyValuePair<Vector3, Vector3>(lines[j].Key, lines[i].Value);
            //        }
            //    }
            //}
        }
    }
}