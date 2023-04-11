using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public abstract class Geometry
{
    protected List<Triangle> triangles = new List<Triangle>();
    protected List<Vector3> vertices = new List<Vector3>();
    protected Dictionary<Vector3, List<Vector3>> pointConnections = new Dictionary<Vector3, List<Vector3>>();
    public List<Vector3> Vertices => vertices;
    public List<Triangle> Triangles => triangles;
    public Dictionary<Vector3, List<Vector3>> PointConnections => pointConnections;
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
        return Vector2.Distance(_point2D, Delaunay.GetVector2(A)) < 0.00001f || Vector2.Distance(_point2D, Delaunay.GetVector2(B)) < 0.00001f || Vector2.Distance(_point2D, Delaunay.GetVector2(C)) < 0.00001f;
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
        if(Mathf.Abs(_distanceP - _radius) < 0.1f)
            Debug.Log(_distanceP +" "+_radius);
        if(_distanceP <= _radius - 0.001f)
            return true;
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
        List<Vector3> _connect = new List<Vector3>();
        _connect.Add(B);
        _connect.Add(C);
        pointConnections.Add(A, new List<Vector3>(_connect));
        _connect.Clear();

        _connect.Add(A);
        _connect.Add(C);
        pointConnections.Add(B, new List<Vector3>(_connect));
        _connect.Clear();

        _connect.Add(A);
        _connect.Add(B);
        if(!pointConnections.ContainsKey(C))
        pointConnections.Add(C, new List<Vector3>(_connect));
        _connect.Clear();
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

    public Edge(Vector3 a, Vector3 b)
    {
        A = a;
        B = b;
        vertices.Add(A);
        vertices.Add(B);
        pointConnections.Add(A, new List<Vector3>() { B });
        pointConnections.Add(B, new List<Vector3>() { A });
    }
    public static implicit operator bool(Edge _e)
    {
        return _e != null;
    }
    public Vector2 GetNormal()
    {
        return Delaunay.GetVector2(A - B).normalized;
    }
}

public class DelaunayGroup : Geometry
{
    
    public DelaunayGroup(List<Triangle> _triangles, List<Vector3> _vertices)
    {
        triangles = _triangles;
        for (int i = 0; i < _vertices.Count; i++)
        {
            pointConnections.Add(_vertices[i], new List<Vector3>());
        }
        foreach (var item in pointConnections)
        {
            for (int i = 0; i < _triangles.Count; i++)
            {
                if(_triangles[i].PointConnections.ContainsKey(item.Key))
                item.Value.AddRange(_triangles[i].PointConnections[item.Key]);
            }
        }
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
    [SerializeField] bool debug = false;
    static int maxIte = 0;
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
        triangles = _geo.Triangles;
        return _geo;
    }
    public Geometry Link(Geometry _leftGeo, Geometry _rightGeo)
    {
        List<Triangle> _triangles = new List<Triangle>(_leftGeo.Triangles);
        List<Vector3> _vertices = new List<Vector3>(_leftGeo.Vertices);
        _vertices.AddRange(_rightGeo.Vertices);
        _triangles.AddRange(_rightGeo.Triangles);

        Vector3 _leftMin = _leftGeo.GetLowestPoint(); 
        Vector3 _rightMin = _rightGeo.GetLowestPoint();

        Edge _edgeLr = new Edge(_leftMin, _rightMin);
        if(_vertices.Count == count)
        Debug.DrawLine(_leftMin + Vector3.up, _rightMin + Vector3.up, Color.red, 10);

        int max = 0;
        int _iteration = 0;
        while (max < 50)
        {
            List<Vector3> npcLeft = new List<Vector3>();
            List<Vector3> npcRight = new List<Vector3>();

            npcLeft.AddRange(_leftGeo.PointConnections[_edgeLr.A]);
            npcRight.AddRange(_rightGeo.PointConnections[_edgeLr.B]);

            npcLeft.OrderBy(v =>
            {
                float _dot = Vector2.Dot(GetVector2(_leftMin - v).normalized, _edgeLr.GetNormal());
                float _angle = Mathf.Acos(_dot) * Mathf.Rad2Deg;
                return _angle;
            });
            npcRight.OrderBy(v =>
            {
                float _dot = Vector2.Dot(GetVector2(_rightMin - v).normalized, _edgeLr.GetNormal());
                float _angle = Mathf.Acos(_dot) * Mathf.Rad2Deg;
                return _angle;
            });

            Triangle _newLeftT = null;
            int _npcLeftId = int.MaxValue;
            for (int i = 0; i < npcLeft.Count; i++)
            {
                float _angle = Vector2.SignedAngle(GetVector2(npcLeft[i] - _edgeLr.A).normalized, _edgeLr.GetNormal());
                if (_angle <= 0)
                {
                    if (_vertices.Count == count)
                    {
                        //Debug.DrawLine(npcLeft[i] + Vector3.up * 0.5f, _edgeLr.A + Vector3.up * 0.5f, Color.green, 10);
                        //Debug.Log(_angle);
                    }
                    continue;
                }
                _newLeftT = new Triangle(_edgeLr.A, _edgeLr.B, npcLeft[i]);
                if (!IsTriangleValid(_newLeftT, _vertices))
                {
                    if (debug && _vertices.Count == count)
                    {
                        Debug.DrawLine(_newLeftT.A + Vector3.up * (0.5f + max), _newLeftT.B + Vector3.up * (0.5f + max), Color.green, 10);
                        Debug.DrawLine(_newLeftT.B + Vector3.up * (0.5f + max), _newLeftT.C + Vector3.up * (0.5f + max), Color.green, 10);
                        Debug.DrawLine(_newLeftT.C + Vector3.up * (0.5f + max), _newLeftT.A + Vector3.up * (0.5f + max), Color.green, 10);
                    }
                    _newLeftT = null;
                    continue;
                }
                _npcLeftId = i;
                break;
            }
            Triangle _newRightT = null;
            int _npcRightId = int.MaxValue;
            for (int i = 0; i < npcRight.Count; i++)
            {
                //if (i >= _npcRightId) break;
                float _angle = Vector2.SignedAngle(GetVector2(npcRight[i] - _edgeLr.B).normalized, _edgeLr.GetNormal());
                if (_angle <= 0)
                {
                    if (_vertices.Count == count)
                    {
                        //Debug.DrawLine(npcRight[i] + Vector3.up * 0.5f, _edgeLr.B + Vector3.up * 0.5f, Color.green, 10);
                        //Debug.Log(_angle);
                    }
                    continue;
                }
                _newRightT = new Triangle(_edgeLr.A, _edgeLr.B, npcRight[i]);
                if (!IsTriangleValid(_newRightT, _vertices))
                {
                    if(debug && _vertices.Count == count)
                    {

                        Debug.DrawLine(_newRightT.A + Vector3.up * (0.5f + max), _newRightT.B + Vector3.up * (0.5f + max), Color.green, 10);
                        Debug.DrawLine(_newRightT.B + Vector3.up * (0.5f + max), _newRightT.C + Vector3.up * (0.5f + max), Color.green, 10);
                        Debug.DrawLine(_newRightT.C + Vector3.up * (0.5f + max), _newRightT.A + Vector3.up * (0.5f + max), Color.green, 10);
                    }
                    _newRightT = null;
                    continue;
                }
                _npcRightId = i;
                break;
            }

            if(_npcLeftId < _npcRightId)
            {
                _triangles.Add(_newLeftT);
                _edgeLr = new Edge(_newLeftT.C, _newLeftT.B);
            }
            else if(_npcRightId < _npcLeftId)
            {
                _triangles.Add(_newRightT);
                _edgeLr = new Edge(_newRightT.A, _newRightT.C);
            }
            else if(_newLeftT)
            {
                _triangles.Add(_newLeftT);
                _edgeLr = new Edge(_newLeftT.C, _newLeftT.B);
            }
            else if (_newRightT)
            {
                _triangles.Add(_newRightT);
                _edgeLr = new Edge(_newRightT.A, _newRightT.C);
            }
            else
            {
                if (_vertices.Count == count)
                    Debug.Log("finish");
                break;
            }
            if (_vertices.Count == count)
                Debug.DrawLine(_edgeLr.A +Vector3.up*1.25f, _edgeLr.B + Vector3.up * 2, Color.magenta, 10);
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
}
