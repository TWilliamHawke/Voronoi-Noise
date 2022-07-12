using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Voronoi
{
    public class Cell
    {
        Vector3 _center;

        public Vector3 center => _center;
        List<Vector3> _vertices = new List<Vector3>();
        public List<Vector3> vertices => _vertices;
        public HashSet<Cell> _neighborCells = new HashSet<Cell>();

        int _verticesBellowY = 0;

        public bool debug = false;

        public Cell(Vector3 position)
        {
            _center = position;
        }

        public void AddNeighbor(Cell other)
        {
            if (other == this || other is null) return;
            _neighborCells.Add(other);
        }

        public void AddTriangle(Triangle triangle)
        {
            if(debug && vertices.Count >= 5) return;
            var newVertex = triangle.center.AddZ();
            _vertices.Add(newVertex);
            Log(newVertex);

            SortVertices();
        }

        //One step of Insertion Sort
        void SortVertices()
        {
            Log($"Round #{_vertices.Count}");

            LogAllVertices();
            int targetIndex = -1;
            if(_vertices.LastOrDefault().y > _center.y)
            {
                targetIndex = FindTargetIndexBetween(0, _verticesBellowY);
                _verticesBellowY++;
            }
            else
            {
                targetIndex = FindTargetIndexBetween(_verticesBellowY, _vertices.Count - 1);
            }
            Log($"Target index is {targetIndex}");
            LogAllVertices();

            if (targetIndex == -1) return;

            for (int i = targetIndex; i < _vertices.Count; i++)
            {
                Swap(i, _vertices.Count - 1);
            }
        }

        private int FindTargetIndexBetween(int from, int to)
        {
            int targetIndex = to;
            var toNewTriangle = _vertices.LastOrDefault() - _center;

            for (int i = from; i < to; i++)
            {
                var toExistingTriangle = _vertices[i] - _center;
                var sin = toNewTriangle.x * toExistingTriangle.y - toExistingTriangle.x * toNewTriangle.y;

                if (sin > 0) continue;
                targetIndex = i;

                break;
            }

            return targetIndex;
        }

        void Swap(int i, int j)
        {
            if (i == j) return;
            var temp = _vertices[i];
            _vertices[i] = _vertices[j];
            _vertices[j] = temp;
        }

        void LogAllVertices()
        {
            foreach(var vertex in _vertices)
            {
                Log(vertex);
            }
        }

        void Log(object obj)
        {
            if(!debug) return;
            Debug.Log(obj);
        }

    }
}

