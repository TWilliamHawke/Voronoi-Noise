using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Voronoi.Helpers
{
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    public class MeshBuilder : MonoBehaviour
    {
        List<Vector3> _vertices = new List<Vector3>();
        List<int> _triangles = new List<int>();

        [SerializeField] MeshRenderer _meshRenderer;
        [SerializeField] MeshFilter _meshFilter;

        [SerializeField] Material _material;
        [SerializeField] bool _useReverseOrder;

        int _reverseMult => _useReverseOrder ? 0 : 1;


        public void AddPolygon(params Vector3[] verticies)
        {
            if (verticies.Length < 3)
            {
                throw new System.Exception("This is not a polygon");
            }

            //it can be not the first polygon
            int startIndex = _vertices.Count;
            _vertices.Add(verticies[0]);

            for (int i = 1; i < verticies.Length - 1; i++)
            {
                _vertices.Add(verticies[i]);

                _triangles.Add(startIndex);
                _triangles.Add(startIndex + i + 1 * _reverseMult);
                _triangles.Add(startIndex + i + 1 * (1 - _reverseMult));
            }

            _vertices.Add(verticies[verticies.Length - 1]);
        }

        public void CreateMesh(string meshName = "GeneratedMesh")
        {
            var mesh = new Mesh();
            mesh.Clear();
            mesh.name = meshName;
            mesh.vertices = _vertices.ToArray();
            mesh.triangles = _triangles.ToArray();
            mesh.RecalculateNormals();
            _meshFilter.sharedMesh = mesh;

            _vertices = null;
            _triangles = null;
        }

        public void SetRandomColor()
        {
            float r = Random.Range(0f, 1f);
            float g = Random.Range(0f, 1f);
            float b = Random.Range(0f, 1f);
            SetMeshColor(new Color(r, g, b, 1f));
        }

        public void SetMeshColor(Color color)
        {
            _meshRenderer.material = _material;
            _meshRenderer.material.color = color;
        }

        //optional. use if you want a round surface
        public void RemoveDublicatedVertices()
        {
            //key - vertice, value - index in new list
            Dictionary<Vector3, int> uniqueVertices = new Dictionary<Vector3, int>(_vertices.Count);

            //key - index in old list, value - index in new list
            Dictionary<int, int> updatedIndexes = new Dictionary<int, int>(_vertices.Count);
            // updatedIndexes.Keys.

            //vertice in this list does not repeat
            List<Vector3> updatedVerticies = new List<Vector3>(_vertices.Count);

            for (int i = 0; i < _vertices.Count; i++)
            {
                var vertice = _vertices[i];

                if (uniqueVertices.TryGetValue(vertice, out var newIndex))
                {
                    //vertice is not unique
                    updatedIndexes.Add(i, newIndex);
                }
                else
                {
                    //vertice found first time, save index in updated list
                    updatedIndexes.Add(i, updatedVerticies.Count);
                    uniqueVertices.Add(vertice, updatedVerticies.Count);
                    updatedVerticies.Add(vertice);
                }
            }

            for (int j = 0; j < _triangles.Count; j++)
            {
                if (updatedIndexes.TryGetValue(_triangles[j], out var newIndex))
                {
                    _triangles[j] = newIndex;
                }
            }

            _vertices = updatedVerticies;
        }
    }
}
