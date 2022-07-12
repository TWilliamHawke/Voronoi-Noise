using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Voronoi.Helpers;

namespace Voronoi
{
    public class Field : MonoBehaviour, ITriangilationField
    {
        [SerializeField] int _seed = 999;
        [SerializeField] int _width = 20;
        [SerializeField] int _height = 20;
        [Range(0f, .4f)]
        [SerializeField] float _gap = .1f;

        [SerializeField] float _cellSize = 10f;

        [Header("Visualisation")]
        [SerializeField] MeshBuilder _meshCreatorPrefab;
        [SerializeField] ShowPoints _showPoints = ShowPoints.inner;
        [SerializeField] bool _showTriangles;
        [Tooltip("Game mod Only")]
        [SerializeField] bool _renderMeshes;

        //tools
        System.Random _rng;
        DelaunayTriangulation _triangulator;

        //collections
        Vector3[,] _pointsArray;
        List<Vector2> _points;
        Dictionary<Vector2, Cell> _cells;

        List<Vector2> ITriangilationField.points => _points;
        int outerWidth => _width + 2;
        int outerHeight => _height + 2;

        void Awake()
        {
            Init();
        }


        void OnValidate()
        {
            if (!gameObject.activeSelf) return;
            Init();
        }

        void OnDrawGizmos()
        {
            if (_triangulator is null) return;

            DrawTriangulationResult();
            DrawCellCenters();
            DrawBordersForAllCells();
        }

        void Init()
        {
            _rng = new System.Random(_seed);
            CreateCells();
            CreateTriangles();

            CreateCellsDataFromTriangles();

            if (Application.isEditor) return;
            RenderMeshes();
        }

        void CreateCellsDataFromTriangles()
        {
            foreach (var triangle in _triangulator.triangles)
            {
                _cells.TryGetValue(triangle.v1, out var cell1);
                _cells.TryGetValue(triangle.v2, out var cell2);
                _cells.TryGetValue(triangle.v3, out var cell3);

                cell1?.AddTriangle(triangle);
                cell2?.AddTriangle(triangle);
                cell3?.AddTriangle(triangle);

                cell1?.AddNeighbor(cell2);
                cell1?.AddNeighbor(cell3);
                cell2?.AddNeighbor(cell3);
                cell2?.AddNeighbor(cell1);
                cell3?.AddNeighbor(cell1);
                cell3?.AddNeighbor(cell2);
            }
        }

        void CreateCells()
        {
            _pointsArray = new Vector3[_width, _height];
            _cells = new Dictionary<Vector2, Cell>(_width * _height);
            //if gap = 0.1 position will be clamped between xx.1 and xx.9
            float gap_mult = 1 - _gap * 2;

            for (int i = 0; i < _width; i++)
            {
                for (int j = 0; j < _height; j++)
                {
                    float x = (i + _gap + (float)_rng.NextDouble() * gap_mult) * _cellSize;
                    float y = (j + _gap + (float)_rng.NextDouble() * gap_mult) * _cellSize;

                    var center = new Vector3(x, y, 0f);

                    _pointsArray[i, j] = center;
                    _cells.Add(center, new Cell(center));
                }
            }
        }

        //UNDONE: should use spawn pool
        void RenderMeshes()
        {
            foreach (Transform children in transform)
            {
                Destroy(children.gameObject);
            }

            if (!_renderMeshes) return;

            foreach (var pair in _cells)
            {
                var cell = pair.Value;
                if (cell.vertices is null) return;

                var meshCreator = Instantiate(_meshCreatorPrefab, Vector3.zero, Quaternion.identity);
                var points = cell.vertices.ToArray();
                meshCreator.AddPolygon(points);
                meshCreator.CreateMesh();
                meshCreator.transform.SetParent(transform);
                meshCreator.SetRandomColor();
            }
        }

        void CreateTriangles()
        {
            _triangulator = new DelaunayTriangulation(this, new GridSorting(outerWidth, outerHeight));
            FillPointsList();

            _triangulator.CreateTriangles();
        }

        void FillPointsList()
        {
            _points = new List<Vector2>(outerWidth * outerHeight);
            FillColumn(0);

            for (int i = 0; i < _height + 2; i++)
            {
                _points[i] = _points[i].MirrorX(0);
            }

            for (int i = 0; i < _width; i++)
            {
                FillColumn(i);
            }

            int lastColumnStartIdx = _points.Count;
            FillColumn(_width - 1);

            for (int i = lastColumnStartIdx; i < _points.Count; i++)
            {
                _points[i] = _points[i].MirrorX(_width * _cellSize);
            }
        }

        void FillColumn(int i)
        {
            Vector3 firstPoint = _pointsArray[i, 0];
            _points.Add(firstPoint.RemoveZ().MirrorY(0));

            for (int j = 0; j < _height; j++)
            {
                _points.Add(_pointsArray[i, j].RemoveZ());
            }

            Vector3 lastPoint = _pointsArray[i, _height - 1];
            _points.Add(lastPoint.RemoveZ().MirrorY(_height * _cellSize));
        }

        void DrawTriangulationResult()
        {
            if (!_showTriangles) return;
            Gizmos.color = Color.green;

            if (_triangulator.edges is null) return;

            foreach (var pair in _triangulator.edges)
            {
                Gizmos.DrawLine(pair.Key.start, pair.Key.end);
            }
        }

        void DrawCellCenters()
        {
            if (_showPoints == ShowPoints.off) return;
            Gizmos.color = Color.red;
            if (_showPoints == ShowPoints.all)
            {
                DrawAllCellCenters();
            }
            else
            {
                DrawInnerCellCenters();
            }
        }

        private void DrawAllCellCenters()
        {
            if (_points is null) return;

            foreach (var point in _points)
            {
                Gizmos.DrawSphere(point, 1f);
            }
        }

        void DrawInnerCellCenters()
        {
            if (_pointsArray is null) return;

            foreach (var point in _pointsArray)
            {
                Gizmos.DrawSphere(point.RemoveZ(), 1f);
            }
        }

        void DrawBordersForAllCells()
        {
            Gizmos.color = Color.blue;

            if (_cells is null) return;

            foreach (var pair in _cells)
            {
                var cell = pair.Value;

                DrawCellBorders(cell);
            }
        }

        static void DrawCellBorders(Cell cell)
        {
            if (cell.vertices is null || cell.vertices.Count == 0) return;
            var startPoint = cell.vertices[cell.vertices.Count - 1];

            for (int i = 0; i < cell.vertices.Count; i++)
            {
                var endPoint = cell.vertices[i];
                Gizmos.DrawLine(startPoint, endPoint);
                startPoint = endPoint;
            }
        }

        enum ShowPoints
        {
            off,
            inner,
            all
        }
    }

}

