using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DelaunayGroup : Geometry
{
    public DelaunayGroup(List<Triangle> _triangles, List<Vector3> _vertices)
    {
        triangles = _triangles;
        for (int i = 0; i < _vertices.Count; i++)
        {
            if (!pointConnections.ContainsKey(_vertices[i]))
                pointConnections.Add(_vertices[i], new List<Vector3>());
        }
        foreach (var item in pointConnections)
        {
            for (int i = 0; i < _triangles.Count; i++)
            {
                if (_triangles[i].PointConnections.ContainsKey(item.Key))
                    item.Value.AddRange(_triangles[i].PointConnections[item.Key]);
            }
        }
        vertices = _vertices;
    }

    public override bool ContainsPoint(Vector3 _point)
    {
        List<Vector3> _list = vertices.FindAll(v => Delaunay.GetVector2(_point) == Delaunay.GetVector2(v));
        if (_list.Count == 0)
            return false;
        return true;
    }

}