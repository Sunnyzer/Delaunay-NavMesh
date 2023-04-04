using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Geometry
{
    public abstract Vector3 GetLowestPoint();
    public abstract Vector3 GetHighestPoint();
}

[Serializable]
public class Triangle : Geometry
{
    public Vector3 A;
    public Vector3 B;
    public Vector3 C;
    public bool ContainsPoint(Vector3 _point)
    {
        return Vector3.Distance(_point, A) < 0.001f || Vector3.Distance(_point, B) < 0.001f || Vector3.Distance(_point, C) < 0.001f;
    }
    public Vector3 GetCenterTriangle()
    {
        float d = (A.x * (B.z - C.z) + B.x * (C.z - A.z) + C.x * (A.z - B.z)) * 2;
        float AX2 = Mathf.Pow(A.x, 2f);
        float AY2 = Mathf.Pow(A.z, 2f);
        float BX2 = Mathf.Pow(B.x, 2f);
        float BY2 = Mathf.Pow(B.z, 2f);
        float CX2 = Mathf.Pow(C.x, 2f);
        float CY2 = Mathf.Pow(C.z, 2f);
        float x = (1/d) * ((AX2 + AY2) * (B.z - C.z) + (BX2 + BY2) * (C.z - A.z) + (CX2 + CY2) * (A.z - B.z));
        float z = (1/d) * ((AX2 + AY2) * (C.x - B.x) + (BX2 + BY2) * (A.x - C.x) + (CX2 + CY2) * (B.x - A.x));
        return new Vector3(x,0, z);
    }

    public override Vector3 GetLowestPoint()
    {
        if (A.z < B.z && A.z < C.z)
            return A;
        if (B.z < A.z && B.z < C.z)
            return B;
        if(C.z < A.z && C.z < B.z)
            return C;
        return A;
    }
    public override Vector3 GetHighestPoint()
    {
        if (A.z > B.z && A.z > C.z)
            return A;
        if (B.z > A.z && B.z > C.z)
            return B;
        if (C.z > A.z && C.z > B.z)
            return C;
        return A;
    }

    public Triangle(Vector3 a, Vector3 b, Vector3 c)
    {
        A = a;
        B = b;
        C = c;
    }
    public static implicit operator bool(Triangle _t)
    {
        return _t != null;
    }
}

public class Edge : Geometry
{
    public Vector3 A;
    public Vector3 B;
    public bool ContainsPoint(Vector3 _point)
    {
        return Vector3.Distance(_point, A) < 0.001f || Vector3.Distance(_point, B) < 0.001f;
    }
    public override Vector3 GetLowestPoint()
    {
        if (A.z > B.z)
            return B;
        else if (A.z < B.z)
            return A;
        else
            return A;
    }
    public override Vector3 GetHighestPoint()
    {
        if (A.z < B.z)
            return B;
        else if (A.z < B.z)
            return A;
        else
            return A;
    }
    public Edge(Vector3 a, Vector3 b)
    {
        A = a;
        B = b;
    }
    public static implicit operator bool(Edge _e)
    {
        return _e != null;
    }
}

public class DelaunayGroup : Geometry
{
    List<Triangle> triangles = new List<Triangle>();
    List<Vector3> vertices = new List<Vector3>();
    public DelaunayGroup(List<Triangle> _triangles, List<Vector3> _vertices)
    {
        triangles = _triangles;
        vertices = _vertices;
    }
    public override Vector3 GetLowestPoint()
    {
        return vertices.OrderBy(v => v.y).First();
    }
    public override Vector3 GetHighestPoint()
    {
        return vertices.OrderBy(v => v.y).Last();
    }
}

public class Delaunay
{
    [SerializeField] List<Triangle> triangles = new List<Triangle>();
    [NonSerialized] public List<Vector3> Vertices = new List<Vector3>();

    public List<Triangle> ComputeDelaunay(List<Vector3> _vertices)
    {
        List<Triangle> result = new List<Triangle>();
        //order 
        _vertices = _vertices.OrderBy((point) => 
        {
            return point.x + point.z * 0.0001f;
        }).ToList();
        
        //Create And Split vertices
        List<Vector3> _tempVertices = new List<Vector3>(_vertices);
        List<Vector3> _tempVertices2 = new List<Vector3>(_vertices);
        _tempVertices.RemoveRange(_vertices.Count/2, _vertices.Count/2);
        _tempVertices2.RemoveRange(0, _vertices.Count/2);

        Edge _leftE = null;
        Triangle _leftT = null;
        Edge _rightE = null;
        Triangle _rightT = null;
        DelaunayGroup _rightGroup = null;
        DelaunayGroup _leftGroup = null;

        if(_tempVertices.Count == 2)
        {
            _leftE = new Edge(_tempVertices[0], _tempVertices[1]);
        }
        else if (_tempVertices.Count == 3)
        {
            _leftT = new Triangle(_tempVertices[0], _tempVertices[1], _tempVertices[2]);
            result.Add(_leftT);
        }
        else
        {
            Debug.Log("Delaunay");
            List<Triangle> _tempTriangles = ComputeDelaunay(_tempVertices);
            _leftGroup = new DelaunayGroup(_tempTriangles, _tempVertices);
        }

        if (_tempVertices2.Count == 2)
        {
            _rightE = new Edge(_tempVertices2[0], _tempVertices2[1]);
        }
        else if (_tempVertices2.Count == 3)
        {
            _rightT = new Triangle(_tempVertices2[0], _tempVertices2[1], _tempVertices2[2]);
            result.Add(_rightT);
        }
        else
        {
            Debug.Log("Delaunay");
            result.AddRange(ComputeDelaunay(_tempVertices2));
        }

        if (_leftE && _rightT)
        {
            Edge _edgeLR = new Edge(_leftE.GetLowestPoint(), _rightT.GetLowestPoint());
            //result.AddRange(Link(_edgeLR, _vertices));
        }
        else if (_leftT && _rightE)
        {
            Edge _edgeLR = new Edge(_leftT.GetLowestPoint(), _rightE.GetLowestPoint());
            //result.AddRange(Link(_edgeLR, _vertices));
        }
        else if (_leftE && _rightE)
        {
            Edge _edgeLR = new Edge(_leftE.GetLowestPoint(), _rightE.GetLowestPoint());
            //result.AddRange(Link(_edgeLR, _vertices));
        }
        else if (_leftT && _rightT)
        {
            if (!IsValidTriangle(_leftT, _vertices))
                result.Remove(_leftT);
            if(!IsValidTriangle(_rightT, _vertices))
                result.Remove(_rightT);

            Edge _edgeLR = new Edge(_leftT.GetLowestPoint(), _rightT.GetLowestPoint());
            Debug.DrawLine(_edgeLR.A, _edgeLR.B + Vector3.up * 0.1f, Color.red, 10);
            Edge _newEdgeLR = null;
            int _countTBeforeIteration = result.Count;
            for (int j = 0; j < 10; j++)
            {
                result.Add(Link(_edgeLR, _vertices, out _newEdgeLR, result.Count != _countTBeforeIteration ? result[result.Count - 1].A : null));
                _edgeLR = _newEdgeLR;
                if (_newEdgeLR)
                    Debug.DrawLine(_newEdgeLR.A, _newEdgeLR.B + Vector3.up * 0.1f, Color.red, 10);
                else
                    break;
            }
            
        }
        Vertices = _vertices;
        triangles.Clear();
        triangles = result;
        return result;
    }
    public Triangle Link(Edge _startEdge, List<Vector3> _vertices, out Edge _edgeLR, Vector3? _toIgnore = null)
    {
        Triangle _newT = null;
        _edgeLR = null;
        int _count = _vertices.Count;
        for (int i = 0; i < _count; i++)
        {
            if (_startEdge.ContainsPoint(_vertices[i])) continue;
            if (_toIgnore != null && Vector3.Distance(_toIgnore.Value, _vertices[i]) < 0.001f) continue;
            _newT = new Triangle(_startEdge.A, _startEdge.B, _vertices[i]);
            Vector3 _center = _newT.GetCenterTriangle();
            Debug.DrawLine(_center, _center + Vector3.up * 0.1f, Color.magenta, 10);

            bool _validT = IsValidTriangle(_newT, _vertices);
            if (!_validT) continue;
            _edgeLR = null;
            if (i >= _count / 2)
            {
                _edgeLR = new Edge(_newT.A, _newT.C);
            }
            else
            {
                _edgeLR = new Edge(_newT.B, _newT.C);
            }
            return _newT;
        }
        return null;
    }
    public bool IsValidTriangle(Triangle _triangle, List<Vector3> _vertices)
    {
        int _count = _vertices.Count;
        Vector3 _center = _triangle.GetCenterTriangle();
        for (int i = 0; i < _count; i++)
        {
            if (_triangle.ContainsPoint(_vertices[i])) continue;
            if (Vector3.Distance(_center, _vertices[i]) < Vector3.Distance(_center, _triangle.A))
            {
                return false;
            }
        }
        return true;
    }
    public void DrawGizmo()
    {
        Gizmos.color = Color.blue;
        for (int i = 0; i < triangles.Count; i++)
        {
            if (triangles[i] == null) continue;
            Gizmos.DrawLine(triangles[i].A, triangles[i].B);
            Gizmos.DrawLine(triangles[i].B, triangles[i].C);
            Gizmos.DrawLine(triangles[i].C, triangles[i].A);
        }
    }
}
