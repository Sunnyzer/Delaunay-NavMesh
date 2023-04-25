using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NavMesh))]
public class NavMeshEditor : Editor
{
    NavMesh eTarget;
    private void OnEnable() => eTarget = (NavMesh)target;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Generate NavMesh"))
            eTarget.CreateNavMesh();
        if (GUILayout.Button("Generate Point"))
            eTarget.GeneratePoint();
    }
    private void OnSceneGUI()
    {
        DisplayVertices(eTarget.Vertices);
        DisplayVoronoi(eTarget.Nodes);
        DisplayTriangle(eTarget.Triangles);
    }
    public void DisplayVertices(List<Vector3> _vertices)
    {
        int _count = _vertices.Count;
        Vector3 _cameraPos = Camera.current.transform.position;
        for (int i = 0; i < _count; i++)
        {
            Vector3 _v = _vertices[i];
            Handles.Label(_v + Vector3.up * 0.5f, i.ToString().ToUpper());
            if (Vector3.Distance(_v, _cameraPos) < 5)
                eTarget.Vertices[i] = Handles.DoPositionHandle(_v, Quaternion.identity);
        }
    }
    public void DisplayVoronoi(List<Node> _nodes)
    {
        Handles.color = eTarget.NavMeshDebug.voronoiColor;
        int _count = _nodes.Count;
        for (int i = 0; i < _count; i++)
        {
            Node _node = _nodes[i];
            Vector3 _position = _node.position; 
            foreach (var neighbor in _node.neighborsIndex)
                Handles.DrawLine(_position, _nodes[neighbor]);
        }
    }
    public void DisplayTriangle(List<Triangle> _triangles)
    {
        Handles.color = eTarget.NavMeshDebug.triangleColor;
        int _count = _triangles.Count;
        for (int i = 0; i < _count; i++)
        {
            Triangle _t = _triangles[i];
            Handles.DrawLine(_t.A, _t.B);
            Handles.DrawLine(_t.B , _t.C);
            Handles.DrawLine(_t.C, _t.A);
        }
    }
}
