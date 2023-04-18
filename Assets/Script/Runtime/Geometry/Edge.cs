using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Edge : Geometry
{
    public Vector3 A;
    public Vector3 B;
    public override bool ContainsPoint(Vector3 _point)
    {
        return Vector3.Distance(_point, A) < 0.001f || Vector3.Distance(_point, B) < 0.001f;
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
        if (!pointConnections.ContainsKey(B))
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