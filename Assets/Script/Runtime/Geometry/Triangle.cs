using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Triangle : Geometry
{
    public Vector3 A;
    public Vector3 B;
    public Vector3 C;
    [NonSerialized] public List<Triangle> neighbors = new List<Triangle>();
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
        if (!pointConnections.ContainsKey(C))
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
        float x = (1 / d) * ((AX2 + AY2) * (B.z - C.z) + (BX2 + BY2) * (C.z - A.z) + (CX2 + CY2) * (A.z - B.z));
        float z = (1 / d) * ((AX2 + AY2) * (C.x - B.x) + (BX2 + BY2) * (A.x - C.x) + (CX2 + CY2) * (B.x - A.x));
        return new Vector3(x, 0, z);
    }
    public bool IsInCircle(Vector3 _point)
    {
        Vector2 _center2D = Delaunay.GetVector2(GetCenterTriangle());
        Vector2 _point2D = Delaunay.GetVector2(_point);
        float _distanceP = Vector2.Distance(_center2D, _point2D);
        float _radius = Vector2.Distance(_center2D, Delaunay.GetVector2(A));
        if (_distanceP <= _radius - 0.001f)
            return true;
        return false;
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
    public void DrawTriangle(Vector3 _offset, float _duration = 0, Color? _color = null)
    {
        Color color = Color.black;
        if (_color != null)
            color = _color.Value;
        Debug.DrawLine(A + _offset, B + _offset, color, _duration);
        Debug.DrawLine(B + _offset, C + _offset, color, _duration);
        Debug.DrawLine(C + _offset, A + _offset, color, _duration);
    }
    public bool IsPointInTriangle(Vector3 _point)
    {
        float _a = (A.x - _point.x) * (B.z - _point.z) - (A.z - _point.z) * (B.x - _point.x);
        float _b = (B.x - _point.x) * (C.z - _point.z) - (B.z - _point.z) * (C.x - _point.x);
        float _c = (C.x - _point.x) * (A.z - _point.z) - (C.z - _point.z) * (A.x - _point.x);
        if (_a >= 0 && _b >= 0 && _c >= 0)
            return true;
        if (_a < 0 && _b < 0 && _c < 0)
            return true;
        return false;
    }
    public static implicit operator bool(Triangle _t)
    {
        return _t != null;
    }
}
