using Retrover.Meshes;
using Retrover.ObjectPool;
using Retrover.Path;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoadMeshNew : MonoBehaviour, IPoolable
{
    [SerializeField] private MeshFilter _filter;
    [SerializeField] private MeshRenderer _renderer;
    private Mesh _mesh;
    private float _roadHalfWidth = 0.5f;
    private List<PathTransform> _pathTransforms = new();

    public void Initialize(List<PathTransform> pathTransforms, float width)
    {
        _roadHalfWidth = width / 2f;
        _pathTransforms.AddRange(pathTransforms);
        GenerateNew();
    }

    public void ReturnToPool()
    {
        _mesh?.Clear();
        _pathTransforms.Clear();
    }

    public void GenerateNew()
    {
        List<RoadShoulders> roadHandles = GenerateHandles(_pathTransforms);
        List<IPolygon> roadPoints = GeneratePoints(roadHandles);
        BuildMesh(roadPoints);
    }

    private RoadShoulders CalculateShoulders(Vector3 point, Vector3 normal)
    {
        Vector3 perpendicular = Vector3.Cross(normal, Vector3.up).normalized;
        Vector3 leftShoulder = point + perpendicular * _roadHalfWidth;
        Vector3 rightShoulder = point - perpendicular * _roadHalfWidth;
        return new RoadShoulders(leftShoulder, rightShoulder);
    }

    private List<RoadShoulders> GenerateHandles(List<PathTransform> transforms)
    {
        List<RoadShoulders> handles = new();
        int lastIndex = transforms.Count - 2;
        for (int i = 0; i <= lastIndex; i++)
        {
            handles.Add(CalculateShoulders(transforms[i].Position, transforms[i].Normal));

            if (i == lastIndex)
                handles.Add(CalculateShoulders(transforms[i + 1].Position, transforms[i + 1].Normal));
            else
                handles.Add(CalculateShoulders(transforms[i + 1].Position, transforms[i].Normal));
        }
        return handles;
    }

    private List<IPolygon> GeneratePoints(List<RoadShoulders> handles)
    {
        List<IPolygon> roadPolygons = new();

        Vector3 currentLeftPoint = handles[0].Left;
        Vector3 currentRightPoint = handles[0].Right;

        for (int i = 0; i < handles.Count - 2; i += 2)
        {
            TwoLines leftLines = new(handles[i].Left, handles[i + 1].Left, handles[i + 2].Left, handles[i + 3].Left);
            TwoLines rightLines = new(handles[i].Right, handles[i + 1].Right, handles[i + 2].Right, handles[i + 3].Right);

            if (leftLines.IsIntersecting())
            {
                Vector3 nextLeftPoint = leftLines.GetIntersectionPoint();
                roadPolygons.Add(new Rectangle(currentLeftPoint, nextLeftPoint, currentRightPoint, handles[i + 1].Right));
                
                //Vector3 newRightPoint = Vector3.Lerp(currentRightPoint, handles[i + 1].Right, 0.5f);
                //Vector3 mainPoint = Vector3.Lerp(handles[i + 1].Left, handles[i + 1].Right, 0.5f);
                //Vector3 mainDirection = (newRightPoint - mainPoint).normalized;
                //newRightPoint = mainPoint + mainDirection * _roadHalfWidth;
                //currentRightPoint = handles[i + 1].Right;
                //roadPolygons.Add(new Triangle(currentLeftPoint, currentRightPoint, newRightPoint));

                currentLeftPoint = nextLeftPoint;
                currentRightPoint = handles[i + 1].Right;
            }
            else if (rightLines.IsIntersecting())
            {
                Vector3 nextRightPoint = rightLines.GetIntersectionPoint();
                roadPolygons.Add(new Rectangle(currentLeftPoint, handles[i + 1].Left, currentRightPoint, nextRightPoint));

                //Vector3 newLeftPoint = Vector3.Lerp(currentLeftPoint, handles[i + 1].Left, 0.5f);
                //Vector3 mainPoint = Vector3.Lerp(handles[i + 1].Left, handles[i + 1].Right, 0.5f);
                //Vector3 mainDirection = (newLeftPoint - mainPoint).normalized;
                //newLeftPoint = mainPoint + mainDirection * _roadHalfWidth;
                //currentLeftPoint = handles[i + 1].Left;
                //roadPolygons.Add(new Triangle(currentLeftPoint, newLeftPoint, currentRightPoint));

                currentLeftPoint = handles[i + 1].Left;
                currentRightPoint = nextRightPoint;
            }
            else
            {
                roadPolygons.Add(new Rectangle(currentLeftPoint, handles[i + 1].Left, currentRightPoint, handles[i + 1].Right));
                currentLeftPoint = handles[i + 1].Left;
                currentRightPoint = handles[i + 1].Right;
            }
        }

        roadPolygons.Add(new Rectangle(currentLeftPoint, handles.Last().Left, currentRightPoint, handles.Last().Right));

        return roadPolygons;
    }

    private void BuildMesh(List<IPolygon> polygons)
    {
        _mesh = new Mesh();
        List<Vector3> vertices = new();
        List<int> triangles = new();
        List<Vector2> uvs = new();
        List<Vector3> normals = new();

        float uvOddset = 0;
        for (int i = 0; i < polygons.Count; i++)
        {
            polygons[i].GenerateForMesh(vertices, triangles, uvs, normals, ref uvOddset);
        }

        _mesh.SetVertices(vertices);
        _mesh.SetTriangles(triangles, 0);
        _mesh.SetUVs(0, uvs);
        _mesh.RecalculateNormals();
        _filter.mesh = _mesh;
    }

    private struct RoadShoulders
    {
        public Vector3 Left;
        public Vector3 Right;

        public RoadShoulders(Vector3 left, Vector3 right)
        {
            Left = left;
            Right = right;
        }
    }

    private struct TwoLines
    {
        public (Vector2, Vector2) line1, line2;
        public float y1, y2;

        public TwoLines(Vector3 point1, Vector3 point2, Vector3 point3, Vector3 point4)
        {
            line1 = (ConvertToVector2(point1), ConvertToVector2(point2));
            line2 = (ConvertToVector2(point3), ConvertToVector2(point4));
            y1 = point2.y;
            y2 = point3.y;
        }

        public Vector3 GetIntersectionPoint()
        {
            Vector2 p = line1.Item1;
            Vector2 r = line1.Item2 - line1.Item1;
            Vector2 q = line2.Item1;
            Vector2 s = line2.Item2 - line2.Item1;
            float t = Vector3.Cross(q - p, s).z / Vector3.Cross(r, s).z;

            Vector2 resultVector = p + t * r;

            return new Vector3(resultVector.x, CalculateAverageY(), resultVector.y);
        }

        public bool IsIntersecting()
        {
            // Реализация проверки на пересечение
            Vector2 r = line1.Item2 - line1.Item1;
            Vector2 s = line2.Item2 - line2.Item1;
            float rxs = Vector3.Cross(r, s).z;

            if (rxs == 0) return false; // Отрезки параллельны

            Vector2 qMinusP = line2.Item1 - line1.Item1;
            float t = Vector3.Cross(qMinusP, s).z / rxs;
            float u = Vector3.Cross(qMinusP, r).z / rxs;

            return (t >= 0 && t <= 1 && u >= 0 && u <= 1);
        }

        private static Vector2 ConvertToVector2(Vector3 point3D)
            => new(point3D.x, point3D.z);

        private float CalculateAverageY()
            => (y1 + y2) / 2;
    }
}