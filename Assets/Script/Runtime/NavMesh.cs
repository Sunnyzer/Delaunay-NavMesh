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
    public static implicit operator Vector3(Node _node) => _node.position;
}

[Serializable]
public struct NavMeshDebug
{
    [SerializeField] public bool displayNavMeshVolume;
    [SerializeField] public Color voronoiColor;
    [SerializeField] public Color triangleColor;
}

[Serializable]
public struct NavMeshSettings
{
    [SerializeField] public Vector3 extendsVolume;
    [SerializeField] public LayerMask navMeshLayer;
    [SerializeField] public LayerMask pointLayer;
    [SerializeField] public LayerMask volumeLayer;
}

public class NavMesh : Singleton<NavMesh>
{
    List<Vector3> vertices = new List<Vector3>();
    [SerializeField] NavMeshData navMeshData;
    [SerializeField] NavMeshDebug navMeshDebug;
    [SerializeField] NavMeshSettings navMeshSettings;

    public NavMeshDebug NavMeshDebug => navMeshDebug;
    public NavMeshSettings NavMeshSettings => navMeshSettings;

    public List<Vector3> Vertices => vertices;
    public List<Triangle> Triangles => navMeshData.triangles;
    public List<Node> Nodes => navMeshData.nodes;

    public void CreateNavMesh()
    {
        Geometry _geometry = new Delaunay().ComputeDelaunay(vertices);
        navMeshData.triangles.Clear();
        navMeshData.triangles = _geometry.Triangles;
        vertices = _geometry.Vertices;
        navMeshData.nodes.Clear();
        navMeshData.triangles = RemoveWrongTriangles(navMeshData.triangles);
        navMeshData.nodes = CreateNode(navMeshData.triangles);
        #if UNITY_EDITOR
        EditorUtility.SetDirty(navMeshData);
        #endif
    }
    public List<Node> CreateNode(List<Triangle> _triangles)
    {
        List<Node> _nodes = new List<Node>();
        int _count = _triangles.Count;
        for (int i = 0; i < _count; ++i)
        {
            Triangle _t = _triangles[i];
            Node _node = new Node((_t.A + _t.B + _t.C) / 3);
            _nodes.Add(_node);
            for (int j = 0; j < _count; ++j)
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
                else if (_a && _c)
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
        return _nodes;
    }
    public List<Triangle> RemoveWrongTriangles(List<Triangle> _triangles)
    {
        int _count = _triangles.Count;
        int i = 0;
        while (i < _count)
        {
            Triangle _t = _triangles[i];
            bool _hitAB = Physics.Linecast(_t.A, _t.B);
            bool _hitBC = Physics.Linecast(_t.B, _t.C);
            bool _hitCA = Physics.Linecast(_t.C, _t.A);
            if (_hitAB || _hitBC || _hitCA)
            {
                _triangles.Remove(_t);
                --_count;
                continue;
            }
            i++;
        }
        return _triangles;
    }
    public void GeneratePoint()
    {
        vertices.Clear();
        Vector3 _extends = navMeshSettings.extendsVolume / 2;
        Vector3 _position = transform.position;
        AddPoint(_position + _extends);
        AddPoint(_position - _extends);
        AddPoint(_position - new Vector3(_extends.x, 0, -_extends.z));
        AddPoint(_position - new Vector3(-_extends.x, 0, _extends.z));
        Collider[] _colliders = Physics.OverlapBox(_position, _extends, transform.rotation, navMeshSettings.volumeLayer);
        int _count = _colliders.Length;
        for (int i = 0; i < _count; i++)
        {
            Handles.color = Color.white;
            Bounds _bounds = _colliders[i].bounds;

            Handles.color = Color.red;
            Vector3 _extendsWithoutY = new Vector3(_bounds.extents.x, 0, _bounds.extents.z);
            Vector3 _extendsOpposite = new Vector3(_bounds.extents.x, 0, -_bounds.extents.z);

            Vector3 _boundPExtends = _bounds.center + _extendsWithoutY;
            Vector3 _boundMExtends = _bounds.center - _extendsWithoutY;
            Vector3 _boundPExtendsOpposite = _bounds.center + _extendsOpposite;
            Vector3 _boundMExtendsOpposite = _bounds.center - _extendsOpposite;

            Vector3 _offset = new Vector3(1, 0, 1) * 0.2f;
            Vector3 _offsetOpposite = new Vector3(1, 0, -1) * 0.2f;

            Vector3[] _allPoint = new Vector3[]
            { 
                //_boundPExtends - _offset,
                //_boundMExtends + _offset,
                //_boundMExtendsOpposite + _offsetOpposite,
                //_boundPExtendsOpposite - _offsetOpposite,
                _boundMExtendsOpposite - _offsetOpposite,
                _boundPExtendsOpposite + _offsetOpposite,
                _boundMExtends - _offset,
                _boundPExtends + _offset,
            };
            foreach (var _point in _allPoint)
                AddPoint(_point);
        }
    }
    public void AddPoint(Vector3 _point)
    {
        bool _hit = Physics.Raycast(_point + Vector3.up * 10000, Vector3.down, out RaycastHit _rayHit, Mathf.Infinity, navMeshSettings.pointLayer);
        if (_hit)
        {
            Vector3 _position = new Vector3((float)Math.Round(_rayHit.point.x, 2), (float)Math.Round(_rayHit.point.y, 2), (float)Math.Round(_rayHit.point.z, 2));
            vertices.Add(_position);
        }
    }
    private void OnDrawGizmos()
    {
        if (!navMeshDebug.displayNavMeshVolume) return;
        Gizmos.color = Color.green - new Color(0,0,0, 0.7f);
        Gizmos.DrawCube(transform.position, navMeshSettings.extendsVolume);
    }
}
