using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Geometry
{
    protected List<Triangle> triangles = new List<Triangle>();
    protected List<Vector3> vertices = new List<Vector3>();
    public List<Vector3> Vertices => vertices;
    public List<Triangle> Triangles => triangles;
    public abstract Vector3 GetLowestPoint();
    public abstract Vector3 GetHighestPoint();
    public abstract bool ContainsPoint(Vector3 _point);
}

[Serializable]
public class Triangle : Geometry
{
    public Vector3 A;
    public Vector3 B;
    public Vector3 C;
    public override bool ContainsPoint(Vector3 _point)
    {
        Vector2 _point2D = Delaunay.GetVector2(_point);
        return Vector2.Distance(_point2D, Delaunay.GetVector2(A)) < 0.01f || Vector2.Distance(_point2D, Delaunay.GetVector2(B)) < 0.00001f || Vector2.Distance(_point2D, Delaunay.GetVector2(C)) < 0.00001f;
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
        return new Vector3(x, 0, z);
    }
    public bool IsInCircle(Vector3 _point)
    {
        Vector2 _center2D = Delaunay.GetVector2(GetCenterTriangle());
        Vector2 _point2D = Delaunay.GetVector2(_point);
        float _distanceP = Vector2.Distance(_center2D, _point2D);
        float _radius = Vector2.Distance(_center2D, Delaunay.GetVector2(A));
        if(_distanceP <= _radius - 0.001f)
            return true;
        //if(Mathf.Abs(_radius - _distanceP) < 0.01f)
        //Debug.Log(_radius + " " + _distanceP);
        return false;
    }
    public override Vector3 GetLowestPoint()
    {
        if (A.z - B.z <= 0.001f && A.z <= 0.001f)
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
        triangles.Add(this);
        vertices.Add(A);
        vertices.Add(B);
        vertices.Add(C);
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
    public override bool ContainsPoint(Vector3 _point)
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
    public Edge(Edge _edge)
    {
        this.A = _edge.A;
        this.B = _edge.B;
    }
    public Edge(Vector3 a, Vector3 b)
    {
        A = a;
        B = b;
        vertices.Add(A);
        vertices.Add(B);
    }
    public static implicit operator bool(Edge _e)
    {
        return _e != null;
    }
}

public class DelaunayGroup : Geometry
{
    
    public DelaunayGroup(List<Triangle> _triangles, List<Vector3> _vertices)
    {
        triangles = _triangles;
        vertices = _vertices;
    }
    public override Vector3 GetLowestPoint()
    {
        return vertices.OrderBy(v => v.z).First();
    }
    public override Vector3 GetHighestPoint()
    {
        return vertices.OrderBy(v => v.z).Last();
    }

    public override bool ContainsPoint(Vector3 _point)
    {
        List<Vector3> _list = vertices.FindAll(a => Vector2.Distance(Delaunay.GetVector2(_point), Delaunay.GetVector2(a)) <= 0.0001f);
        if(_list.Count == 0)
            return false;
        return true;
    }
}

[Serializable]
public class Delaunay
{
    [SerializeField] List<Triangle> triangles = new List<Triangle>();
    [NonSerialized] public List<Vector3> Vertices = new List<Vector3>();
    [SerializeField] bool _debug = false;
    int count = 0;
    public static Vector2 GetVector2(Vector3 _point)
    {
        return new Vector2(_point.x, _point.z);
    }
    public Geometry ComputeDelaunay(List<Vector3> _vertices)
    {
        count = _vertices.Count;
        _vertices = _vertices.OrderBy((point) => 
        {
            return point.x + point.z * 0.0001f;
        }).ToList();
        return ComputeDelaunayWithoutOrder(_vertices);
    }
    private Geometry ComputeDelaunayWithoutOrder(List<Vector3> _vertices)
    {
        //Create And Split vertices
        List<Vector3> _verticesLeft = new List<Vector3>(_vertices);
        List<Vector3> _verticesRight = new List<Vector3>(_vertices);
        int _count = _vertices.Count / 2;
        if (_vertices.Count % 2 != 0)
            _verticesLeft.RemoveRange(_count, _count + 1);
        else
            _verticesLeft.RemoveRange(_count, _count);

        _verticesRight.RemoveRange(0, _count);

        Geometry _leftGeo = null;
        Geometry _rightGeo = null;
        if (_verticesLeft.Count == 1 && _verticesRight.Count == 1)
            return new Edge(_verticesLeft[0], _verticesRight[0]);
        else if (_verticesLeft.Count == 1 && _verticesRight.Count == 2)
            return new Triangle(_verticesLeft[0], _verticesRight[0], _verticesRight[1]);
        else if (_verticesLeft.Count == 2 && _verticesRight.Count == 1)
            return new Triangle(_verticesLeft[0], _verticesLeft[1], _verticesRight[0]);

        if (_verticesLeft.Count == 2)
            _leftGeo = new Edge(_verticesLeft[0], _verticesLeft[1]);
        else if (_verticesLeft.Count == 3)
            _leftGeo = new Triangle(_verticesLeft[0], _verticesLeft[1], _verticesLeft[2]);
        else
            _leftGeo = ComputeDelaunayWithoutOrder(_verticesLeft);

        if (_verticesRight.Count == 2)
            _rightGeo = new Edge(_verticesRight[0], _verticesRight[1]);
        else if (_verticesRight.Count == 3)
            _rightGeo = new Triangle(_verticesRight[0], _verticesRight[1], _verticesRight[2]);
        else
            _rightGeo = ComputeDelaunayWithoutOrder(_verticesRight);

        Geometry _geo = Link(_leftGeo, _rightGeo);
        return _geo;
    }
    public Geometry Link(Geometry _leftGeo, Geometry _rightGeo)
    {
        List<Triangle> _triangles = new List<Triangle>();
        List<Vector3> _vertices = new List<Vector3>(_leftGeo.Vertices);
        _vertices.AddRange(_rightGeo.Vertices);
        _vertices = _vertices.OrderBy((point) =>
        {
            return point.x + point.z * 0.0001f;
        }).ToList();

        _triangles.AddRange(_leftGeo.Triangles);
        _triangles.AddRange(_rightGeo.Triangles);

        Vector3 _leftMin = _leftGeo.GetLowestPoint(); 
        Vector3 _rightMin = _rightGeo.GetLowestPoint();

        Edge _edgeLr = new Edge(_leftMin, _rightMin);
        if(_vertices.Count != count && _debug)
            Debug.DrawLine(_leftMin, _rightMin + Vector3.up, Color.red ,10);
        
        int max = 0;
        int h = 1;
        Vector3? _toIgnore = null;
        bool _0t = true;
        List<Triangle> _miss = new List<Triangle>();
        while(max < 20 && !(_edgeLr.ContainsPoint(_leftGeo.GetHighestPoint()) && _edgeLr.ContainsPoint(_rightGeo.GetHighestPoint())))
        {
            List<Vector3> _verticesUp = _vertices.FindAll(v => v.z >= _edgeLr.A.z || v.z >= _edgeLr.B.z);
            for (int i = 0; i < _verticesUp.Count; i++)
            {
                _0t = true;
                Vector3 _vertice = _verticesUp[i];
                if (_edgeLr.ContainsPoint(_vertice)) continue;
                if (_toIgnore != null && Vector2.Distance(GetVector2(_toIgnore.Value), GetVector2(_vertice)) <= 0.001f) continue;
                Triangle _t = new Triangle(_edgeLr.A, _edgeLr.B, _vertice);
                bool _isValid = IsTriangleValid(_t, _vertices);
                if (!_isValid)
                {
                    _miss.Add(_t);
                    continue;
                }
                _triangles.Add(_t);
                if (_leftGeo.ContainsPoint(_vertice))
                {
                    _toIgnore = _edgeLr.A;
                    _edgeLr = new Edge(_vertice, _edgeLr.B);
                }
                else
                {
                    _toIgnore = _edgeLr.B;
                    _edgeLr = new Edge(_edgeLr.A, _vertice);
                }
                if(_vertices.Count != count && _debug)
                    Debug.DrawLine(_edgeLr.A + Vector3.up * h, _edgeLr.B + Vector3.up * h, Color.blue, 10);
                _0t = false;
                h++;
                break;
            }
            if(_0t)
            {
                if(_debug)
                {
                    for (int i = 0; i < _miss.Count; i++)
                    {
                        Debug.DrawLine(_miss[i].A + Vector3.up * (i + 1), _miss[i].B + Vector3.up * (i + 1), Color.green, 10);
                        Debug.DrawLine(_miss[i].B + Vector3.up * (i + 1), _miss[i].C + Vector3.up * (i + 1), Color.green, 10);
                        Debug.DrawLine(_miss[i].C + Vector3.up * (i + 1), _miss[i].A + Vector3.up * (i + 1), Color.green, 10);
                        Debug.DrawLine(_miss[i].GetCenterTriangle(), _miss[i].C + Vector3.up * (i + 1), Color.magenta, 10);
                        Vector2 _center = GetVector2(_miss[i].GetCenterTriangle());
                        float _distA = Vector2.Distance(_center, GetVector2(_miss[i].A));
                        float _distB = Vector2.Distance(_center, GetVector2(_miss[i].B));
                        if (_distA == _distB && _distA == Vector2.Distance(_center, _miss[i].C))
                            Debug.Log("Test");
                    }
                }   
                break;
            }
            max++;
        }
        for (int j = 0; j < _triangles.Count; j++)
        {
            Triangle _t = _triangles[j];
            for (int i = 0; i < _vertices.Count; i++)
            {
                if (_t.ContainsPoint(_vertices[i])) continue;
                if (_t.IsInCircle(_vertices[i]))
                {
                    _triangles.Remove(_t);
                    j--;
                    break;
                }
            }
        }
        return new DelaunayGroup(_triangles, _vertices);
    }
    public bool IsTriangleValid(Triangle _t, List<Vector3> _vertices)
    {
        for (int i = 0; i < _vertices.Count; i++)
        {
            Vector3 _v = _vertices[i];
            if (_t.ContainsPoint(_v)) continue;
            if (_t.IsInCircle(_v))
            {
                return false;
            }
        }
        return true;
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

            bool _validT = false;//= //IsValidTriangle(_newT, _vertices);
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
}
