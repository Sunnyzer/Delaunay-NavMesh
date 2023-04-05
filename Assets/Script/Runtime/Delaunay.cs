using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
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
        return Vector3.Distance(_point, A) < 0.00001f || Vector3.Distance(_point, B) < 0.00001f || Vector3.Distance(_point, C) < 0.00001f;
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
        Vector3 _center = GetCenterTriangle();
        if (ContainsPoint(_point))
            return false;
        if(Vector3.Distance(_center, _point) > Vector3.Distance(_center, A))
            return false;
        return true;
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
        List<Vector3> _list = vertices.FindAll(a => Vector3.Distance(_point, a) < 0.001f);
        if(_list.Count != 0)
            return true;
        return false;
    }
}

public class Delaunay
{
    [SerializeField] List<Triangle> triangles = new List<Triangle>();
    [NonSerialized] public List<Vector3> Vertices = new List<Vector3>();

    public Geometry ComputeDelaunay(List<Vector3> _vertices)
    {
        _vertices = _vertices.OrderBy((point) => 
        {
            return point.x + point.z * 0.0001f;
        }).ToList();
        
        //Create And Split vertices
        List<Vector3> _verticesLeft = new List<Vector3>(_vertices);
        List<Vector3> _verticesRight = new List<Vector3>(_vertices);
        int _count = _vertices.Count / 2;
        if(_vertices.Count % 2 != 0)
            _verticesLeft.RemoveRange(_count, _count + 1);
        else
            _verticesLeft.RemoveRange(_count, _count);

        _verticesRight.RemoveRange(0, _count);
        Geometry _leftGeo = null;
        Geometry _rightGeo = null;

        if(_verticesLeft.Count == 2)
        {
            //Debug.Log("Create Left Edge : " + _verticesLeft[0] + " " + _verticesLeft[1]);
            _leftGeo = new Edge(_verticesLeft[0], _verticesLeft[1]);
        }
        else if (_verticesLeft.Count == 3)
        {
            //Debug.Log("Create Left Triangle : " + _verticesLeft[0] + " " + _verticesLeft[1] + " " + _verticesLeft[2]);
            _leftGeo = new Triangle(_verticesLeft[0], _verticesLeft[1], _verticesLeft[2]);
        }
        else
        {
            //Debug.Log("Create Left Delaunay Count V : " + _verticesLeft.Count);
            _leftGeo = ComputeDelaunay(_verticesLeft);
            //Debug.Log("After Create Left V : " + _leftGeo.Vertices.Count);
        }

        if (_verticesRight.Count == 2)
        {
            //Debug.Log("Create Right Edge : " + _verticesRight[0] + " " + _verticesRight[1]);
            _rightGeo = new Edge(_verticesRight[0], _verticesRight[1]);
        }
        else if (_verticesRight.Count == 3)
        {
            //Debug.Log("Create Right Triangle : " + _verticesRight[0] + " " + _verticesRight[1] + " " + _verticesRight[2]);
            _rightGeo = new Triangle(_verticesRight[0], _verticesRight[1], _verticesRight[2]);
        }
        else
        {
            //Debug.Log("Create Right Delaunay V Count :" + _verticesRight.Count);
            _rightGeo = ComputeDelaunay(_verticesRight);
            //Debug.Log("After Create Right V Count : " + _rightGeo.Vertices.Count);
        }

        if (_leftGeo != null && _rightGeo != null)
        {
            Geometry _geo = Link(_leftGeo, _rightGeo);
            //Debug.Log(_geo.Vertices.Count + " " + _geo.Triangles.Count);
            return _geo;
        }
        else
            Debug.Log("left or right geo null");
        return null;
    }
    public Geometry Link(Geometry _leftGeo, Geometry _rightGeo)
    {
        List<Triangle> _triangles = new List<Triangle>();
        List<Vector3> _vertices = new List<Vector3>(_leftGeo.Vertices);
        _vertices.AddRange(_rightGeo.Vertices);

        _triangles.AddRange(_leftGeo.Triangles);
        _triangles.AddRange(_rightGeo.Triangles);

        Vector3 _leftMin = _leftGeo.GetLowestPoint(); 
        Vector3 _rightMin = _rightGeo.GetLowestPoint();
        Edge _edgeLr = new Edge(_leftMin, _rightMin);
        Debug.DrawLine(_edgeLr.A + Vector3.up * 0.1f, _edgeLr.B + Vector3.up * 0.1f, Color.red,10);
        bool _stopLink = false;
        int max = 0;
        int h = 1;
        Vector3? _toIgnore = null;
        List<Triangle> missT = new List<Triangle>();
        while (!_stopLink && max < 10)
        {
            List<Vector3> _verticesUp = _vertices.FindAll(v => v.z > _edgeLr.A.z || v.z > _edgeLr.B.z);
            int _count = _verticesUp.Count;
            for (int i = 0; i < _count; i++)
            {
                if (_edgeLr.ContainsPoint(_verticesUp[i])) continue;
                if (_toIgnore != null && Vector3.Distance(_toIgnore.Value, _verticesUp[i]) < 0.001f) continue;
                float _dot = Vector3.Dot((_edgeLr.B - _edgeLr.A).normalized, (_edgeLr.B - _verticesUp[i]).normalized);
                if (_dot >= 0.99f) continue;
                if (_dot <= -0.99f) continue;

                Triangle _newTriangle = new Triangle(_edgeLr.A, _edgeLr.B, _verticesUp[i]);
                bool _isNewTriangleValid = true;
                for (int j = 0; j < _vertices.Count; j++)
                {
                    if (_newTriangle.IsInCircle(_vertices[j]))
                    {
                        _isNewTriangleValid = false;
                        break;
                    }
                }
                if (!_isNewTriangleValid)
                {
                    if (_vertices.Count == 8)
                    {
                        missT.Add(_newTriangle);
                    }
                    continue;
                }
                _triangles.Add(_newTriangle);
                bool _containsLeft = _edgeLr.ContainsPoint(_leftGeo.GetHighestPoint());
                bool _containsRight = _edgeLr.ContainsPoint(_rightGeo.GetHighestPoint());
                if (_leftGeo.ContainsPoint(_verticesUp[i]))
                {
                    _toIgnore = _edgeLr.A;
                    _edgeLr = new Edge(_verticesUp[i], _edgeLr.B);
                }
                else
                {
                    _toIgnore = _edgeLr.B;
                    _edgeLr = new Edge(_edgeLr.A, _verticesUp[i]);
                }
                if (_vertices.Count == 8)
                    Debug.DrawLine(_edgeLr.A + Vector3.up * ((max+1) * 2 + h) * 0.05f, _edgeLr.B + Vector3.up * ((max + 1) * 2 + h) * 0.05f, Color.blue, 10);
                h++;
                if (_containsLeft && _containsRight)
                {
                    Debug.Log("Contains");
                    _stopLink = true;
                    break;
                }
            }
            ++max;
        }
        for (int j = 0; j < _triangles.Count; j++)
        {
            Triangle _t = _triangles[j];
            for (int i = 0; i < _vertices.Count; i++)
            {
                if (_t.IsInCircle(_vertices[i]))
                {
                    _triangles.Remove(_t);
                    j--;
                    break;
                }
            }
        }
        if (_vertices.Count == 8)
        {
            for (int i = 0; i < missT.Count; i++)
            {
                Debug.DrawLine(missT[i].A + Vector3.up * i * 0.5f, missT[i].B + Vector3.up * i * 0.5f, Color.black, 10);
                Debug.DrawLine(missT[i].B + Vector3.up * i * 0.5f, missT[i].C + Vector3.up * i * 0.5f, Color.black, 10);
                Debug.DrawLine(missT[i].C + Vector3.up * i * 0.5f, missT[i].A + Vector3.up * i * 0.5f, Color.black, 10);
            }
            Debug.Log("Triangles : " + _triangles.Count);
        }
        return new DelaunayGroup(_triangles, _vertices);
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
