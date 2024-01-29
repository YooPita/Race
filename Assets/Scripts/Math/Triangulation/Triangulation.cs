using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Retrover.Math
{
    public class Triangulation
    {
        public TriangulationData Data { get; private set; }

        private TriangulationCalculator _calculator;

        public Triangulation(HashSet<Vector2> points)
        {
            Data = new TriangulationData(points);
            _calculator = new TriangulationCalculator(Data);
        }

        public void Calculate()
        {
            _calculator.Calculate();
        }
    }

    public class TriangulationCalculator
    {
        private readonly TriangulationData _data;

        public TriangulationCalculator(TriangulationData data)
        {
            _data = data ?? throw new ArgumentNullException(nameof(data));
        }

        public void Calculate()
        {
            InitializeTriangulation();
            TriangulatePoints();
            RemoveSuperTriangle();
        }

        private void InitializeTriangulation()
        {
            _data.AddTriangle(_data.SuperTriangle);
        }

        private void TriangulatePoints()
        {
            foreach (Vector2 point in _data.Points)
                InsertNewPointInTriangulation(point);
        }

        private void InsertNewPointInTriangulation(Vector2 point)
        {
            HalfEdgeFace2 face = FindTriangleContainingPoint(point);
            SplitTriangleFaceAtPoint(face, point);
            RestoreDelaunayTriangulation(point);
        }

        private HalfEdgeFace2 FindTriangleContainingPoint(Vector2 point)
        {
            foreach (var face in _data.Faces)
                if (IsPointInsideTriangle(face, point))
                    return face;

            throw new InvalidOperationException("No triangle found containing the point");
        }

        private bool IsPointInsideTriangle(HalfEdgeFace2 face, Vector2 point)
        {
            var vertices = face.GetVertices().ToList();
            return GeometryUtils.IsPointInsideTriangle(vertices[0].Position, vertices[1].Position, vertices[2].Position, point);
        }

        private void SplitTriangleFaceAtPoint(HalfEdgeFace2 face, Vector2 splitPosition)
        {
            // Разделение треугольника новой точкой
            IEnumerable<HalfEdge2> newEdges = face.SplitFace(splitPosition);
            _data.AddEdges(newEdges);
        }

        private void RestoreDelaunayTriangulation(Vector2 point)
        {
            // Восстановление триангуляции Делоне
            var edgeStack = new Stack<HalfEdge2>(_data.GetEdgesAroundPoint(point));

            while (edgeStack.Count > 0)
            {
                HalfEdge2 edge = edgeStack.Pop();
                if (ShouldFlipEdge(edge))
                {
                    var flippedEdges = edge.Flip();
                    foreach (var flippedEdge in flippedEdges)
                    {
                        if (!_data.IsEdgeOnStack(flippedEdge, edgeStack))
                        {
                            edgeStack.Push(flippedEdge);
                        }
                    }
                }
            }
        }

        private bool ShouldFlipEdge(HalfEdge2 edge)
        {
            // Проверка, нужно ли переворачивать ребро
            return GeometryUtils.ShouldFlipEdge(edge);
        }

        private void RemoveSuperTriangle()
        {
            // Удаление супертреугольника
            _data.RemoveSuperTriangleVertices();
        }
    }

    public class TriangulationData
    {
        public HashSet<HalfEdgeVertex2> Vertices { get; private set; } = new HashSet<HalfEdgeVertex2>();
        public HashSet<HalfEdgeFace2> Faces { get; private set; } = new HashSet<HalfEdgeFace2>();
        public HashSet<HalfEdge2> Edges { get; private set; } = new HashSet<HalfEdge2>();
        public HashSet<Vector2> Points { get; private set; }
        public Triangle2 SuperTriangle { get; private set; }

        public TriangulationData(HashSet<Vector2> points)
        {
            Points = points ?? throw new ArgumentNullException(nameof(points));
            InitializeSuperTriangle();
        }

        private void InitializeSuperTriangle()
        {
            SuperTriangle = new Triangle2(new Vector2(-100f, -100f), new Vector2(100f, -100f), new Vector2(0f, 100f));
            AddTriangle(SuperTriangle);
        }

        public void AddTriangle(Triangle2 triangle)
        {
            // Создание вершин
            HalfEdgeVertex2 vertex1 = new(triangle.Point1);
            HalfEdgeVertex2 vertex2 = new(triangle.Point2);
            HalfEdgeVertex2 vertex3 = new(triangle.Point3);

            // Добавление вершин, если их ещё нет в наборе
            AddVertexIfNotExists(vertex1);
            AddVertexIfNotExists(vertex2);
            AddVertexIfNotExists(vertex3);

            // Создание рёбер
            HalfEdge2 edge1 = new(vertex1);
            HalfEdge2 edge2 = new(vertex2);
            HalfEdge2 edge3 = new(vertex3);

            // Установка связей между рёбрами
            edge1.NextEdge = edge2;
            edge1.PreviousEdge = edge3;
            edge2.NextEdge = edge3;
            edge2.PreviousEdge = edge1;
            edge3.NextEdge = edge1;
            edge3.PreviousEdge = edge2;

            // Создание грани
            HalfEdgeFace2 face = new(edge1);

            // Установка связи между рёбрами и гранью
            edge1.Face = face;
            edge2.Face = face;
            edge3.Face = face;

            // Добавление рёбер и грани в наборы
            Edges.Add(edge1);
            Edges.Add(edge2);
            Edges.Add(edge3);
            Faces.Add(face);
        }

        public void AddEdges(IEnumerable<HalfEdge2> edges)
        {
            foreach (var edge in edges)
                Edges.Add(edge);
        }

        public IEnumerable<HalfEdge2> GetEdgesAroundPoint(Vector2 point)
        {
            return Edges.Where(edge => edge.Vertex.Position.Equals(point));
        }

        public bool IsEdgeOnStack(HalfEdge2 edge, Stack<HalfEdge2> edgeStack)
        {
            return edgeStack.Contains(edge);
        }

        public void RemoveSuperTriangleVertices()
        {
            var superTriangleVertices = new HashSet<Vector2>
            {
                SuperTriangle.Point1,
                SuperTriangle.Point2,
                SuperTriangle.Point3
            };

            Faces.RemoveWhere(face => face.GetVertices().Any(v => superTriangleVertices.Contains(v.Position)));
            Vertices.RemoveWhere(vertex => superTriangleVertices.Contains(vertex.Position));
        }

        private void AddVertexIfNotExists(HalfEdgeVertex2 vertex)
        {
            if (!Vertices.Contains(vertex))
                Vertices.Add(vertex);
        }
    }

    public static class GeometryUtils
    {
        private const float Epsilon = 0.00001f;

        public static bool IsPointInsideTriangle(Vector2 a, Vector2 b, Vector2 c, Vector2 p)
        {
            float area = TriangleArea(a, b, c);
            float area1 = TriangleArea(p, b, c);
            float area2 = TriangleArea(a, p, c);
            float area3 = TriangleArea(a, b, p);

            return Mathf.Abs(area - (area1 + area2 + area3)) < Epsilon;
        }

        private static float TriangleArea(Vector2 a, Vector2 b, Vector2 c)
        {
            return Mathf.Abs((a.x * (b.y - c.y) + b.x * (c.y - a.y) + c.x * (a.y - b.y)) / 2.0f);
        }

        public static bool ShouldFlipEdge(HalfEdge2 edge)
        {
            if (edge.OppositeEdge == null)
                return false;

            Vector2 p1 = edge.Vertex.Position;
            Vector2 p2 = edge.NextEdge.Vertex.Position;
            Vector2 p3 = edge.PreviousEdge.Vertex.Position;
            Vector2 pOpposite = edge.OppositeEdge.NextEdge.Vertex.Position;

            return IsPointInsideCircumcircle(p1, p2, p3, pOpposite);
        }

        public static bool IsPointToTheRightOrOnLine(Vector2 a, Vector2 b, Vector2 p)
        {
            return GetPointInRelationToVectorValue(a, b, p) <= Epsilon;
        }

        private static float GetPointInRelationToVectorValue(Vector2 a, Vector2 b, Vector2 p)
        {
            float deltaX1 = a.x - p.x;
            float deltaY1 = a.y - p.y;
            float deltaX2 = b.x - p.x;
            float deltaY2 = b.y - p.y;

            return deltaX1 * deltaY2 - deltaX2 * deltaY1;
        }

        private static bool IsPointInsideCircumcircle(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p)
        {
            float[,] matrix = {
            { p1.x - p.x, p1.y - p.y, (p1.x - p.x) * (p1.x - p.x) + (p1.y - p.y) * (p1.y - p.y) },
            { p2.x - p.x, p2.y - p.y, (p2.x - p.x) * (p2.x - p.x) + (p2.y - p.y) * (p2.y - p.y) },
            { p3.x - p.x, p3.y - p.y, (p3.x - p.x) * (p3.x - p.x) + (p3.y - p.y) * (p3.y - p.y) }
        };

            return Determinant(matrix) > 0;
        }

        private static float Determinant(float[,] matrix)
        {
            return matrix[0, 0] * (matrix[1, 1] * matrix[2, 2] - matrix[1, 2] * matrix[2, 1]) -
                   matrix[0, 1] * (matrix[1, 0] * matrix[2, 2] - matrix[1, 2] * matrix[2, 0]) +
                   matrix[0, 2] * (matrix[1, 0] * matrix[2, 1] - matrix[1, 1] * matrix[2, 0]);
        }
    }

    public class HalfEdge2
    {
        public HalfEdgeVertex2 Vertex { get; set; }
        public HalfEdgeFace2 Face { get; set; }
        public HalfEdge2 NextEdge { get; set; }
        public HalfEdge2 OppositeEdge { get; set; }
        public HalfEdge2 PreviousEdge { get; set; }

        public HalfEdge2(HalfEdgeVertex2 vertex)
        {
            Vertex = vertex ?? throw new ArgumentNullException(nameof(vertex));
        }

        public List<HalfEdge2> Flip()
        {
            // Убедимся, что есть противоположное ребро для переворачивания
            if (OppositeEdge == null)
            {
                throw new InvalidOperationException("Cannot flip an edge without an opposite edge.");
            }

            // Получаем соседние рёбра
            HalfEdge2 a = this;
            HalfEdge2 b = a.NextEdge;
            HalfEdge2 c = b.NextEdge;
            HalfEdge2 d = OppositeEdge;
            HalfEdge2 e = d.NextEdge;
            HalfEdge2 f = e.NextEdge;

            // Получаем вершины
            HalfEdgeVertex2 p1 = b.Vertex;
            HalfEdgeVertex2 p2 = d.Vertex;
            HalfEdgeVertex2 p3 = a.Vertex;
            HalfEdgeVertex2 p4 = c.Vertex;

            // Получаем грани
            HalfEdgeFace2 face1 = a.Face;
            HalfEdgeFace2 face2 = d.Face;

            // Переориентируем рёбра
            a.Vertex = p4;
            b.Vertex = p2;
            d.Vertex = p1;
            e.Vertex = p3;

            // Обновляем связи рёбер
            a.NextEdge = e;
            a.PreviousEdge = f;

            d.NextEdge = b;
            d.PreviousEdge = c;

            b.NextEdge = c;
            b.PreviousEdge = d;

            e.NextEdge = f;
            e.PreviousEdge = a;

            c.NextEdge = d;
            c.PreviousEdge = b;

            f.NextEdge = a;
            f.PreviousEdge = e;

            // Обновляем грани
            face1.Edge = a;
            face2.Edge = d;

            a.Face = face1;
            b.Face = face2;
            c.Face = face2;

            d.Face = face1;
            e.Face = face1;
            f.Face = face2;

            return new List<HalfEdge2> { this, NextEdge, PreviousEdge, OppositeEdge, OppositeEdge.NextEdge, OppositeEdge.PreviousEdge };
        }
    }

    public class HalfEdgeVertex2
    {
        public Vector2 Position { get; private set; }
        public HalfEdge2 Edge { get; set; }

        public HalfEdgeVertex2(Vector2 position)
        {
            Position = position;
        }
    }

    public class HalfEdgeFace2
    {
        public HalfEdge2 Edge { get; set; }

        public HalfEdgeFace2(HalfEdge2 edge)
        {
            Edge = edge ?? throw new ArgumentNullException(nameof(edge));
        }

        public IEnumerable<HalfEdgeVertex2> GetVertices()
        {
            yield return Edge.Vertex;
            yield return Edge.NextEdge.Vertex;
            yield return Edge.PreviousEdge.Vertex;
        }

        public IEnumerable<HalfEdge2> SplitFace(Vector2 splitPosition)
        {
            // Создание новой вершины в позиции разделения
            HalfEdgeVertex2 newVertex = new(splitPosition);

            // Получение исходных рёбер треугольника
            HalfEdge2 originalEdge1 = Edge;
            HalfEdge2 originalEdge2 = Edge.NextEdge;
            HalfEdge2 originalEdge3 = Edge.PreviousEdge;

            // Создание новых рёбер
            HalfEdge2 newEdge1 = new(newVertex);
            HalfEdge2 newEdge2 = new(newVertex);
            HalfEdge2 newEdge3 = new(newVertex);

            // Установка новых связей между рёбрами
            newEdge1.NextEdge = originalEdge1.NextEdge;
            newEdge1.PreviousEdge = newEdge2;

            newEdge2.NextEdge = originalEdge2.NextEdge;
            newEdge2.PreviousEdge = newEdge3;

            newEdge3.NextEdge = originalEdge3.NextEdge;
            newEdge3.PreviousEdge = newEdge1;

            originalEdge1.NextEdge.PreviousEdge = newEdge1;
            originalEdge2.NextEdge.PreviousEdge = newEdge2;
            originalEdge3.NextEdge.PreviousEdge = newEdge3;

            originalEdge1.NextEdge = newEdge1;
            originalEdge2.NextEdge = newEdge2;
            originalEdge3.NextEdge = newEdge3;

            // Создание новых граней
            HalfEdgeFace2 newFace1 = new(newEdge1);
            HalfEdgeFace2 newFace2 = new(newEdge2);
            HalfEdgeFace2 newFace3 = new(newEdge3);

            // Установка связей между рёбрами и гранями
            newEdge1.Face = newFace1;
            newEdge2.Face = newFace2;
            newEdge3.Face = newFace3;

            originalEdge1.Face = newFace1;
            originalEdge2.Face = newFace2;
            originalEdge3.Face = newFace3;

            // Возврат новых рёбер
            return new List<HalfEdge2> { newEdge1, newEdge2, newEdge3 };
        }
    }
}
