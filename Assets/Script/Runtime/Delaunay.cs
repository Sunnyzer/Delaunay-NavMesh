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
    public virtual Vector3 GetLowestPointRight()
    {
        List<Vector3> _test = new List<Vector3>(vertices);
        return _test.OrderBy(v => v.z - v.x * 0.001f).First();
    }
    public virtual Vector3 GetLowestPointLeft()
    {
        List<Vector3> _test = new List<Vector3>(vertices);
        return _test.OrderBy(v => v.z + v.x * 0.001f).First();
    }
    public abstract Vector3 GetHighestPoint();
    public abstract bool ContainsPoint(Vector3 _point);
}

[Serializable]
public class Triangle : Geometry
{
    public Vector3 A;
    public Vector3 B;
    public Vector3 C;
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
        if(_distanceP <= _radius - 0.001f)
            return true;
        return false;
    }
    //public override Vector3 GetLowestPointRight()
    //{
    //    List<Vector3> _test = new List<Vector3>(vertices);
    //    return _test.OrderBy(v => v.z - v.x * 0.7f).First();
    //}
    //public override Vector3 GetLowestPointLeft()
    //{
    //    List<Vector3> _test = new List<Vector3>(vertices);
    //    return _test.OrderBy(v => v.z + v.x * 0.7f).First();
    //}
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
    public void DrawTriangle(Vector3 _offset, float _duration = 0, Color? _color = null)
    {
        Color color = Color.black;
        if (_color != null)
            color = _color.Value;
        Debug.DrawLine(A + _offset, B + _offset, color, _duration);
        Debug.DrawLine(B + _offset, C + _offset, color, _duration);
        Debug.DrawLine(C + _offset, A + _offset, color, _duration);
    }

    public static implicit operator bool(Triangle _t)
    {
        return _t != null;
    }
}
[Serializable]
public class Edge : Geometry
{
    public Vector3 A;
    public Vector3 B;
    public override bool ContainsPoint(Vector3 _point)
    {
        return Vector3.Distance(_point, A) < 0.001f || Vector3.Distance(_point, B) < 0.001f;
    }
    //public override Vector3 GetLowestPointRight()
    //{
    //    List<Vector3> _test = new List<Vector3>(vertices);
    //    return _test.OrderBy(v => v.z - v.x * 0.001f).First();
    //}
    //public override Vector3 GetLowestPointLeft()
    //{
    //    List<Vector3> _test = new List<Vector3>(vertices);
    //    return _test.OrderBy(v => v.z + v.x * 0.7f).First();
    //}
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
        if(!pointConnections.ContainsKey(B))
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
[Serializable]
public class DelaunayGroup : Geometry
{
    
    public DelaunayGroup(List<Triangle> _triangles, List<Vector3> _vertices)
    {
        triangles = _triangles;
        for (int i = 0; i < _vertices.Count; i++)
        {
            if(!pointConnections.ContainsKey(_vertices[i]))
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
    //public override Vector3 GetLowestPointRight()
    //{
    //    List<Vector3> _test = new List<Vector3>(vertices);
    //    return _test.OrderBy(v => v.z - v.x * 0.7f).First();
    //}
    //public override Vector3 GetLowestPointLeft()
    //{
    //    List<Vector3> _test = new List<Vector3>(vertices);
    //    return _test.OrderBy(v => v.z + v.x * 0.7f).First();
    //}
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
    [SerializeField] bool inverse = false;
    [SerializeField] int maxIte = 3;
    [SerializeField] int debugDuration = 0;
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
    public List<Vector3> SplitVertices(List<Vector3> _toSplit, out List<Vector3> _rightSplit)
    {
        int _count = _toSplit.Count / 2;
        List<Vector3> _verticesLeft = _toSplit.GetRange(0, _count);
        _rightSplit = _toSplit.GetRange(_count, _count + (_toSplit.Count % 2 == 0 ? 0 : 1));
        return _verticesLeft;
    }
    public List<T> RecombineList<T>(List<T> _toRecombineA, List<T> _toRecombineB)
    {
        List<T> _list = new List<T>(_toRecombineA);
        _list.AddRange(_toRecombineB);
        return _list;
    }
    public Geometry GiveGeo(List<Vector3> _vertices)
    {
        Geometry _geo = null;
        if (_vertices.Count == 2)
            _geo = new Edge(_vertices[0], _vertices[1]);
        else if (_vertices.Count == 3)
            _geo = new Triangle(_vertices[0], _vertices[1], _vertices[2]);
        else
            _geo = ComputeDelaunayWithoutOrder(_vertices);
        return _geo;
    }
    private Geometry ComputeDelaunayWithoutOrder(List<Vector3> _vertices)
    {
        //Create And Split vertices
        List<Vector3> _verticesLeft = SplitVertices(_vertices, out List<Vector3> _verticesRight);

        Geometry _leftGeo = GiveGeo(_verticesLeft);
        Geometry _rightGeo = GiveGeo(_verticesRight);

        Geometry _geo = Link(_leftGeo, _rightGeo);
        triangles = _geo.Triangles;
        return _geo;
    }
    float GetAngle(Vector3 _normalA, Vector3 _normalB)
    {
        float _dot = Vector2.Dot(_normalA, _normalB);
        float _angle = Mathf.Acos(_dot) * Mathf.Rad2Deg;
        return _angle;
    }
    bool IsAngleValid(float _angle)
    {
        return _angle > 0 && _angle < 179.9f;
    }
    private bool IsActiveDebug(int _count)
    {
        return debug && (inverse ? !(_count == count) : _count == count);
    }
    public Geometry Link(Geometry _leftGeo, Geometry _rightGeo)
    {
        List<Triangle> _triangles = RecombineList(_leftGeo.Triangles, _rightGeo.Triangles);
        List<Vector3> _vertices = RecombineList(_leftGeo.Vertices, _rightGeo.Vertices);

        Vector3 _leftMin = _leftGeo.GetLowestPointLeft();
        Vector3 _rightMin = _rightGeo.GetLowestPointRight();

        Edge _edgeLr = new Edge(_leftMin, _rightMin);
        Edge _edgeTest = new Edge(_leftMin, _leftGeo.Vertices.OrderBy(v => v.z + v.x * 0.001f).ToArray()[1]);
        if (Vector2.SignedAngle(_edgeLr.GetNormal(), _edgeTest.GetNormal()) < 0)
            _edgeLr = new Edge(_leftGeo.Vertices.OrderBy(v => v.z + v.x * 0.001f).ToArray()[1], _rightMin);
        //Debug.Log();

        if(IsActiveDebug(_vertices.Count))
        {
            if(Vector2.SignedAngle(_edgeLr.GetNormal(), _edgeTest.GetNormal()) < 0)
            {
                Debug.DrawLine(_leftMin + Vector3.up, _leftGeo.Vertices.OrderBy(v => v.z + v.x * 0.001f).ToArray()[1] + Vector3.up, Color.blue, debugDuration);
            }
            else
            Debug.DrawLine(_leftMin + Vector3.up, _rightMin + Vector3.up, Color.red, debugDuration);
        }

        int max = 0;
        int _iteration = 0;
        while (max < 100)
        {
            List<Vector3> npcLeft = new List<Vector3>();
            List<Vector3> npcRight = new List<Vector3>();

            npcLeft.AddRange(_leftGeo.PointConnections[_edgeLr.A]);
            npcRight.AddRange(_rightGeo.PointConnections[_edgeLr.B]);

            Vector2 _edgeNormal = _edgeLr.GetNormal();
            npcLeft.OrderBy(v => GetAngle(GetVector2(_edgeLr.A - v).normalized, _edgeNormal));
            npcRight.OrderBy(v => GetAngle(GetVector2(_edgeLr.B - v).normalized, _edgeNormal));

            Triangle _newLeftT = null;
            int _npcLeftId = int.MaxValue;
            for (int i = 0; i < npcLeft.Count; i++)
            {
                float _angle = Vector2.SignedAngle(GetVector2(npcLeft[i] - _edgeLr.A).normalized, _edgeLr.GetNormal());
                if (!IsAngleValid(_angle))
                {
                    continue;
                }
                _newLeftT = new Triangle(_edgeLr.A, _edgeLr.B, npcLeft[i]);
                if (!IsTriangleValid(_newLeftT, _vertices))
                {
                    if (IsActiveDebug(_vertices.Count) && _iteration > maxIte)
                    {
                        _newLeftT.DrawTriangle(Vector3.up * (0.5f + max), debugDuration, Color.green);
                    }
                    _newLeftT = null;
                    _npcLeftId = int.MaxValue;
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
                if (!IsAngleValid(_angle))
                {
                    continue;
                }
                _newRightT = new Triangle(_edgeLr.A, _edgeLr.B, npcRight[i]);
                if (!IsTriangleValid(_newRightT, _vertices))
                {
                    if(IsActiveDebug(_vertices.Count) && _iteration > maxIte)
                    {
                        _newRightT.DrawTriangle(Vector3.up * (0.5f + max), debugDuration, Color.green);
                    }
                    _newRightT = null;
                    _npcRightId = int.MaxValue;
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
                break;
            }
            _iteration++;
            if (debug && (inverse ? !(_vertices.Count == count) : _vertices.Count == count))
            {

                Debug.DrawLine(_edgeLr.A + Vector3.up * (1.25f + _iteration), _edgeLr.B + Vector3.up * (2 + _iteration), Color.magenta, debugDuration);
            }
            max++;
        }
        _triangles = RemoveBadTriangles(_triangles, _vertices);
        return new DelaunayGroup(_triangles, _vertices);
    }
    List<Triangle> RemoveBadTriangles(List<Triangle> _triangles, List<Vector3> _vertices)
    {
        int i = 0;
        int _count = _vertices.Count;
        for (int j = 0; j < _triangles.Count; j++)
        {
            Triangle _t = _triangles[j];
            if (_t.A.x == _t.B.x && _t.A.x == _t.C.x || _t.A.z == _t.B.z && _t.A.z == _t.C.z)
            {
                _triangles.Remove(_t);
                j--;
                continue;
            }
            i = 0;
            for (; i < _count; i++)
            {
                Vector3 _v = _vertices[i];
                if (_t.ContainsPoint(_v)) continue;
                Vector2 _center2D = GetVector2(_t.GetCenterTriangle());
                Vector2 _point2D = GetVector2(_v);
                float _distanceP = Vector2.Distance(_center2D, _point2D);
                float _radius = Vector2.Distance(_center2D, Delaunay.GetVector2(_t.A));
                if (_distanceP <= _radius - 0.001f)
                {
                    _triangles.Remove(_t);
                    --j;
                    break;
                }
            }
        }
        return _triangles;
    }
    public bool IsTriangleValid(Triangle _t, List<Vector3> _vertices)
    {
        int _count = _vertices.Count;
        for (int i = 0; i < _count; i++)
        {
            Vector3 _v = _vertices[i];
            if (_t.ContainsPoint(_v)) continue;
            if (_t.IsInCircle(_v)) return false;
        }
        return true;
    }
}
