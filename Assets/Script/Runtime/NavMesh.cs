using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[Serializable]
public class Node
{
    public Vector3 position;
    public List<Edge> neighborsEdge = new List<Edge>();
    public List<int> neighborsIndex = new List<int>();
    public Node(Vector3 _center)
    {
        position = _center;
    }
    public static implicit operator Vector3(Node _node)
    {
        return _node.position;
    }
}

public class NavMesh : Singleton<NavMesh>
{
    [SerializeField] Delaunay delaunay = new Delaunay();
    [SerializeField] List<Vector3> vertices = new List<Vector3>();
    [SerializeField] Vector3 extends = Vector3.one;
    [SerializeField] bool navMeshVolumeDebug = true;
    [SerializeField] LayerMask layerNav;
    //[SerializeField] List<Node> path = new List<Node>();
    [SerializeField] NavMeshData navMeshData;

    public List<Vector3> Vertices => vertices;
    public List<Triangle> Triangles => navMeshData.triangles;
    public Vector3 Extends => extends;
    public LayerMask Layer => layerNav;
    public List<Node> Path => navMeshData.nodes;

    private void FixedUpdate()
    {
        
    }
    public void Compute()
    {
        Geometry _geometry = delaunay.ComputeDelaunay(vertices);
        List<Triangle> _triangles = _geometry.Triangles;
        List<Node> _path = new List<Node>();
        vertices = _geometry.Vertices;
        for (int i = 0; i < _triangles.Count; )
        {
            Triangle _t = _triangles[i];
            bool _hitAB = Physics.Linecast(_t.A, _t.B);
            bool _hitBC = Physics.Linecast(_t.B, _t.C);
            bool _hitCA = Physics.Linecast(_t.C, _t.A);
            if (_hitAB || _hitBC || _hitCA)
            {
                _triangles.Remove(_t);
                continue;
            }
            i++;
        }
        navMeshData.nodes.Clear();
        navMeshData.triangles.Clear();
        for (int i = 0; i < _triangles.Count; i++)
        {
            Triangle _t = _triangles[i];
            Node _node = new Node((_t.A + _t.B + _t.C) / 3);
            _path.Add(_node);
            for (int j = 0; j < _triangles.Count; j++)
            {
                Triangle _neighbor = _triangles[j];
                bool _a = _t.ContainsPoint(_neighbor.A);
                bool _b = _t.ContainsPoint(_neighbor.B);
                bool _c = _t.ContainsPoint(_neighbor.C);
                if (_a && _b)
                {
                    _node.neighborsIndex.Add(j);
                    _node.neighborsEdge.Add(new Edge(_neighbor.A, _neighbor.B));
                }
                else if(_a && _c)
                {
                    _node.neighborsIndex.Add(j);
                    _node.neighborsEdge.Add(new Edge(_neighbor.A, _neighbor.C));
                }
                else if (_c && _b)
                {
                    _node.neighborsIndex.Add(j);
                    _node.neighborsEdge.Add(new Edge(_neighbor.B, _neighbor.C));
                }
            }
        }
        navMeshData.triangles = _triangles;
        navMeshData.nodes = _path;
        EditorUtility.SetDirty(navMeshData);
    }
    private void OnDrawGizmos()
    {
        if (!navMeshVolumeDebug) return;
        Gizmos.color = Color.green - new Color(0,0,0, 0.7f);
        Gizmos.DrawCube(transform.position, extends);
    }
}
