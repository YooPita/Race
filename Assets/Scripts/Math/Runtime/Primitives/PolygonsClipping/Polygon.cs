using System.Collections.Generic;
using UnityEngine;

namespace Retrover.Math.PolygonsClipping
{
    public class Polygon
    {
        public Vertex FirstVertex;

        public void AddVertex(Vector2 position)
        {
            var newVertex = new Vertex(position);
            if (FirstVertex == null)
            {
                FirstVertex = newVertex;
                FirstVertex.Next = FirstVertex;
                FirstVertex.Prev = FirstVertex;
            }
            else
            {
                var lastVertex = FirstVertex.Prev;
                lastVertex.Next = newVertex;
                newVertex.Prev = lastVertex;
                newVertex.Next = FirstVertex;
                FirstVertex.Prev = newVertex;
            }
        }
    }

    public class Vertex
    {
        public Vector2 Position; // Координаты
        public Vertex Next, Prev, Neighbour; // Ссылки на другие вершины
        public bool Intersect, EntryExit; // Флаги пересечения и входа/выхода
        public float Alpha; // Параметр альфа для точки пересечения
        public bool Processed = false;

        public Vertex(Vector2 position)
        {
            Position = position;
        }
    }

    public class Algorithm
    {
        public bool Intersect(Vertex p1, Vertex p2, Vertex q1, Vertex q2, out float alphaP, out float alphaQ)
        {
            Vector2 s = p2.Position - p1.Position;
            Vector2 q = q2.Position - q1.Position;
            Vector2 pq = q1.Position - p1.Position;
            float crossSAndQ = s.x * q.y - s.y * q.x;

            if (Mathf.Abs(crossSAndQ) < Mathf.Epsilon)
            {
                alphaP = alphaQ = 0;
                return false; // Параллельные или совпадающие линии
            }

            float sCrossPQ = pq.x * s.y - pq.y * s.x;
            alphaP = sCrossPQ / crossSAndQ;

            float qCrossPQ = pq.x * q.y - pq.y * q.x;
            alphaQ = qCrossPQ / crossSAndQ;

            return alphaP >= 0 && alphaP <= 1 && alphaQ >= 0 && alphaQ <= 1;
        }

        public void FindAndSortIntersections(Polygon subjectPolygon, Polygon clipPolygon)
        {
            Vertex sStart = subjectPolygon.FirstVertex;
            Vertex cStart = clipPolygon.FirstVertex;

            Vertex s = sStart;
            do
            {
                Vertex nextS = s.Next;
                Vertex c = cStart;
                do
                {
                    Vertex nextC = c.Next;

                    if (Intersect(s, nextS, c, nextC, out float alphaS, out float alphaC))
                    {
                        Vector2 intersectionPoint = s.Position + alphaS * (nextS.Position - s.Position);

                        Vertex intersectionS = new Vertex(intersectionPoint) { Intersect = true, Alpha = alphaS };
                        Vertex intersectionC = new Vertex(intersectionPoint) { Intersect = true, Alpha = alphaC, Neighbour = intersectionS };
                        intersectionS.Neighbour = intersectionC;

                        InsertSorted(subjectPolygon, s, intersectionS);
                        InsertSorted(clipPolygon, c, intersectionC);
                    }

                    c = nextC;
                } while (c != cStart);

                s = nextS;
            } while (s != sStart);
        }

        private void InsertSorted(Polygon polygon, Vertex startVertex, Vertex newVertex)
        {
            Vertex current = startVertex;
            while (current.Alpha < newVertex.Alpha && current.Next != polygon.FirstVertex && current.Next.Alpha < newVertex.Alpha)
            {
                current = current.Next;
            }

            // Вставляем новую вершину после текущей.
            newVertex.Next = current.Next;
            current.Next.Prev = newVertex;
            current.Next = newVertex;
            newVertex.Prev = current;
        }

        public void DetermineEntryExitPoints(Polygon subjectPolygon, Polygon clipPolygon)
        {
            // Устанавливаем начальный статус для точек пересечения в subjectPolygon.
            Vertex current = subjectPolygon.FirstVertex;
            bool inside = IsPointInsidePolygon(current, clipPolygon);
            do
            {
                if (current.Intersect)
                {
                    current.EntryExit = !inside; // Если мы внутри, следующее пересечение должно быть выходом, и наоборот.
                    inside = !inside; // Переключаем внутреннее состояние, так как мы прошли через пересечение.
                }
                current = current.Next;
            } while (current != subjectPolygon.FirstVertex);

            // Повторяем ту же логику для clipPolygon.
            current = clipPolygon.FirstVertex;
            inside = IsPointInsidePolygon(current, subjectPolygon);
            do
            {
                if (current.Intersect)
                {
                    current.EntryExit = !inside;
                    inside = !inside;
                }
                current = current.Next;
            } while (current != clipPolygon.FirstVertex);
        }

        public bool IsPointInsidePolygon(Vertex point, Polygon polygon)
        {
            bool result = false;
            Vertex current = polygon.FirstVertex;
            do
            {
                Vertex next = current.Next;
                if (((current.Position.y > point.Position.y) != (next.Position.y > point.Position.y)) &&
                    (point.Position.x < (next.Position.x - current.Position.x) * (point.Position.y - current.Position.y) / (next.Position.y - current.Position.y) + current.Position.x))
                {
                    result = !result;
                }
                current = next;
            } while (current != polygon.FirstVertex);

            return result;
        }

        public List<Polygon> ConstructResultingPolygons(Polygon subjectPolygon)
        {
            List<Polygon> resultingPolygons = new List<Polygon>();

            // Функция для создания нового полигона и добавления его в список результатов
            void AddPolygon(Vertex start)
            {
                Polygon newPolygon = new Polygon();
                Vertex current = start;
                do
                {
                    newPolygon.AddVertex(current.Position);
                    current.Processed = true; // Отмечаем вершину как обработанную
                    if (current.Intersect)
                    {
                        current = current.Neighbour; // Переходим к соседней вершине в другом полигоне
                    }
                    current = current.EntryExit ? current.Next : current.Prev;
                } while (!current.Processed);
                resultingPolygons.Add(newPolygon);
            }

            Vertex startVertex = subjectPolygon.FirstVertex;
            do
            {
                if (startVertex.Intersect && !startVertex.Processed)
                {
                    AddPolygon(startVertex);
                }
                startVertex = startVertex.Next;
            } while (startVertex != subjectPolygon.FirstVertex);

            return resultingPolygons;
        }
    }
    

}
