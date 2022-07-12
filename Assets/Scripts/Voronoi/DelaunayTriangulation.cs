using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Voronoi
{
    public class DelaunayTriangulation
    {
        ITriangilationField _field;
        ISortingAlgorithm _sortingAlgorithm;

        LinkedList<Vector2> _border = new LinkedList<Vector2>();
        Dictionary<Edge, Diagonoid> _edges = new Dictionary<Edge, Diagonoid>();
        HashSet<Triangle> _triangles;

        public HashSet<Triangle> triangles => _triangles;
        public Dictionary<Edge, Diagonoid> edges => _edges;

        bool _debug = false;

        public DelaunayTriangulation(ITriangilationField field, ISortingAlgorithm sortingAlgorithm)
        {
            _field = field;
            _sortingAlgorithm = sortingAlgorithm;
        }

        public void CreateTriangles()
        {
            if (_field.points is null || _field.points.Count < 3) throw new System.Exception("Not enough cells!");

            //sort from lowest x to hightest
            _sortingAlgorithm.SortPoints(_field.points);

            CreateEdges();
            FillTrianglesList();
        }

        void CreateEdges()
        {
            LinkedListNode<Vector2> lastPointNode;

            lastPointNode = CreateFirstTriangle();
            int maxIdx = _field.points.Count;
            //maxIdx = 110;

            for (int i = 3; i < maxIdx; i++)
            {
                Vector2 point = _field.points[i];
                LinkedListNode<Vector2> visibleNodeBefore = lastPointNode;
                LinkedListNode<Vector2> visibleNodeAfter = lastPointNode;

                while (visibleNodeAfter.Next != null)
                {
                    Vector2 toPoint = (point - visibleNodeAfter.Value);
                    Vector2 toNextPoint = (visibleNodeAfter.Next.Value - visibleNodeAfter.Value);

                    //if next point is unvisible -> break
                    if (toNextPoint.x * toPoint.y - toPoint.x * toNextPoint.y > 0) break;

                    CheckEdge(visibleNodeAfter.Value, visibleNodeAfter.Next.Value, point);
                    visibleNodeAfter = visibleNodeAfter.Next;
                }

                while (visibleNodeBefore.Previous != null)
                {
                    Vector2 toPoint = point - visibleNodeBefore.Value;
                    Vector2 toPrevPoint = visibleNodeBefore.Previous.Value - visibleNodeBefore.Value;

                    //if prev point is unvisible -> break
                    if (toPrevPoint.x * toPoint.y - toPoint.x * toPrevPoint.y < 0) break;

                    CheckEdge(visibleNodeBefore.Value, visibleNodeBefore.Previous.Value, point);
                    visibleNodeBefore = visibleNodeBefore.Previous;
                }

                //delete all nodes between before and after
                var nextNode = visibleNodeBefore.Next;
                while (nextNode != null && nextNode != visibleNodeAfter)
                {
                    if (visibleNodeBefore == visibleNodeAfter) break;

                    var nextPointer = nextNode;
                    nextNode = nextNode.Next;
                    _border.Remove(nextPointer);
                }

                lastPointNode = _border.AddAfter(visibleNodeBefore, point);
            }

            _debug = false;
        }

        LinkedListNode<Vector2> CreateFirstTriangle()
        {
            _border.Clear();
            var e0 = new Edge(_field.points[1], _field.points[2]);
            var e1 = new Edge(_field.points[0], _field.points[2]);
            var e2 = new Edge(_field.points[0], _field.points[1]);

            var p0 = _field.points[0];
            var p1 = _field.points[1];
            var p2 = _field.points[2];

            _edges[e2] = new Diagonoid(e2, p2);
            _edges[e0] = new Diagonoid(e0, p0);
            _edges[e1] = new Diagonoid(e1, p1);

            float dot0 = Vector2.Dot(Vector2.up, (p0 - p2).normalized);
            float dot1 = Vector2.Dot(Vector2.up, (p1 - p2).normalized);

            //point closer to up vector is next
            //point closer to down vector is prev
            var prevPoint = dot0 < dot1 ? p0 : p1;
            var nextPoint = prevPoint == p0 ? p1 : p0;

            //point2 has max x so it is always visible
            var visiblePointNode = _border.AddFirst(_field.points[2]);
            var prevPointNode = _border.AddBefore(visiblePointNode, prevPoint);
            var nextPointNode = _border.AddAfter(visiblePointNode, nextPoint);

            //emulate a circle linked list
            //one eadge in border is always unvisible so we don`t need a real circle list
            _border.AddBefore(prevPointNode, nextPoint);
            _border.AddAfter(nextPointNode, prevPoint);
            return visiblePointNode;
        }

        void CheckEdge(Vector2 start, Vector2 end, Vector2 point)
        {
            var mainEdge = new Edge(start, end);            //  p
            var edgeToStart = new Edge(start, point);       // / \
            var edgeToEnd = new Edge(end, point);           //s---e

            if (_edges.TryGetValue(mainEdge, out var oldDiagonoid))
            {
                Diagonoid testDiagonoid = oldDiagonoid.CreateAltVersion(point);
                var flippedDiagonal = new Edge(point, testDiagonoid.v2);
                var flippedDiagonoid = new Diagonoid(flippedDiagonal, testDiagonoid.edgeEnd, testDiagonoid.edgeStart);

                if (!testDiagonoid.IsComlete() || testDiagonoid.MainEdgeIsCorrect() || flippedDiagonoid.VerticesFromSameSide())
                {
                    _edges[mainEdge] = testDiagonoid;
                    ReplaceOrCreateDiagonoid(end, edgeToStart);
                    ReplaceOrCreateDiagonoid(start, edgeToEnd);
                }
                else
                {
                    _edges.Remove(mainEdge);

                    if (flippedDiagonoid.VerticesFromSameSide())
                    {
                        Debug.LogError(flippedDiagonoid);
                    }

                    if (testDiagonoid.v2 == Vector2.zero)
                    {
                        Debug.LogError(point);
                    }

                    _edges[flippedDiagonal] = flippedDiagonoid;
                    ReplaceOrCreateDiagonoid(testDiagonoid.v2, edgeToEnd);
                    ReplaceOrCreateDiagonoid(testDiagonoid.v2, edgeToStart);

                    CheckEdge(start, testDiagonoid.v2, point);
                    CheckEdge(end, testDiagonoid.v2, point);
                }
            }
            else
            {
                Debug.LogError($"Not found edge: {mainEdge}");
                Debug.Log(point);
            }

        }


        void ReplaceOrCreateDiagonoid(Vector2 point, Edge edge)
        {
            if (_edges.TryGetValue(edge, out var diagonoid))
            {
                _edges[edge] = diagonoid.CreateFullVersion(point);

                if (_edges[edge].v1 == _edges[edge].v2)
                {
                    Debug.LogError($"Vertices are equal: {edge}");
                }
            }
            else
            {
                _edges[edge] = new Diagonoid(edge, point);
            }
        }

        void FillTrianglesList()
        {
            _triangles = new HashSet<Triangle>();

            foreach (var pair in _edges)
            {
                var firstTriangle = new Triangle(pair.Value.edgeStart, pair.Value.edgeEnd, pair.Value.v1);
                _triangles.Add(firstTriangle);
                if (!pair.Value.IsComlete()) continue;

                var secondTriangle = new Triangle(pair.Value.edgeStart, pair.Value.edgeEnd, pair.Value.v2);
                _triangles.Add(secondTriangle);
            }
        }

        void Log(object obj)
        {
            if (!_debug) return;
            Debug.Log(obj);
        }

    }

    public interface ITriangilationField
    {
        List<Vector2> points { get; }
    }

    public interface ISortingAlgorithm
    {
        void SortPoints(List<Vector2> points);
    }
}

