using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
        Edge _edgeL = new Edge(_leftMin, _leftGeo.Vertices.OrderBy(v => v.z + v.x * 0.001f).ToArray()[1]);
        Edge _edgeR = new Edge(_rightGeo.Vertices.OrderBy(v => v.z - v.x * 0.001f).ToArray()[1], _rightMin);
        if (Vector2.SignedAngle(_edgeLr.GetNormal(), _edgeL.GetNormal()) < 0)
            _edgeLr = new Edge(_leftGeo.Vertices.OrderBy(v => v.z + v.x * 0.001f).ToArray()[1], _rightMin);
        else if (Vector2.SignedAngle(_edgeLr.GetNormal(), _edgeR.GetNormal()) > 0)
            _edgeLr = new Edge(_leftMin, _rightGeo.Vertices.OrderBy(v => v.z - v.x * 0.001f).ToArray()[1]);

        if (IsActiveDebug(_vertices.Count))
        {
            if(Vector2.SignedAngle(_edgeLr.GetNormal(), _edgeL.GetNormal()) < 0)
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
            float _angleL = float.MaxValue;
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
                    continue;
                }
                _angleL = _angle;
                break;
            }
            Triangle _newRightT = null;
            float _angleR = float.MaxValue;
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
                    continue;
                }
                _angleR = _angle;
                break;
            }

            if(_angleL < _angleR)
            {
                _triangles.Add(_newLeftT);
                _edgeLr = new Edge(_newLeftT.C, _newLeftT.B);
            }
            else if(_angleL > _angleR)
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
