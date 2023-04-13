using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Node
{
    public Vector3 position;
    public Dictionary<Edge, int> neighbors = new Dictionary<Edge, int>();
    public Node(Vector3 _center)
    {
        position = _center;
    }
    public static implicit operator Vector3(Node _node)
    {
        return _node.position;
    }
}

public class DrawDelaunay : MonoBehaviour
{
    [SerializeField] Delaunay delaunay = new Delaunay();
    [SerializeField] List<Vector3> vertices = new List<Vector3>();
    [SerializeField] Vector3 extends = Vector3.one;
    [SerializeField] bool navMeshVolumeDebug = true;
    [SerializeField] LayerMask layerNav;
    [SerializeField] List<Node> path = new List<Node>();
    public List<Vector3> Vertices => vertices;
    public List<Triangle> Triangles { get; set; }
    public Vector3 Extends => extends;
    public LayerMask Layer => layerNav;
    public List<Node> Path => path;

    private void FixedUpdate()
    {
        
    }
    public void Compute()
    {
        Geometry _geometry = delaunay.ComputeDelaunay(vertices);
        Triangles = _geometry.Triangles;
        vertices = _geometry.Vertices;
        for (int i = 0; i < Triangles.Count; )
        {
            Triangle _t = Triangles[i];
            bool _hitAB = Physics.Linecast(_t.A, _t.B);
            bool _hitBC = Physics.Linecast(_t.B, _t.C);
            bool _hitCA = Physics.Linecast(_t.C, _t.A);
            if (_hitAB || _hitBC || _hitCA)
            {
                Triangles.Remove(_t);
                continue;
            }
            i++;
        }
        path.Clear();
        for (int i = 0; i < Triangles.Count; i++)
        {
            Triangle _t = Triangles[i];
            Node _node = new Node((_t.A + _t.B + _t.C) / 3);
            path.Add(_node);
            for (int j = 0; j < Triangles.Count; j++)
            {
                Triangle _neighbor = Triangles[j];
                bool _a = _t.ContainsPoint(_neighbor.A);
                bool _b = _t.ContainsPoint(_neighbor.B);
                bool _c = _t.ContainsPoint(_neighbor.C);
                if (_a && _b)
                    _node.neighbors.Add(new Edge(_neighbor.A, _neighbor.B), j);
                else if(_a && _c) 
                    _node.neighbors.Add(new Edge(_neighbor.A, _neighbor.C), j);
                else if (_c && _b)
                    _node.neighbors.Add(new Edge(_neighbor.B, _neighbor.C), j);
            }
        }
    }
    private void OnDrawGizmos()
    {
        if (!navMeshVolumeDebug) return;
        Gizmos.color = Color.green - new Color(0,0,0, 0.7f);
        Gizmos.DrawCube(transform.position, extends);
    }
}
