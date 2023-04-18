using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Edge : Geometry
{
    public Vector3 A;
    public Vector3 B;
    public Edge(Vector3 a, Vector3 b)
    {
        A = a;
        B = b;
        vertices.Add(A);
        vertices.Add(B);
        pointConnections.Add(A, new List<Vector3>() { B });
        if (!pointConnections.ContainsKey(B))
            pointConnections.Add(B, new List<Vector3>() { A });
    }

    public override bool ContainsPoint(Vector3 _point)
    {
        //return Vector3.Distance(_point, A) < 0.001f || Vector3.Distance(_point, B) < 0.001f;
        return _point == A || _point == B;
    }

    public Vector2 GetNormal() => Delaunay.GetVector2(A - B).normalized;
    public static implicit operator bool(Edge _e) => _e != null;

}