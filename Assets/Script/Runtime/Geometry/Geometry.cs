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
    public abstract bool ContainsPoint(Vector3 _point);
}
