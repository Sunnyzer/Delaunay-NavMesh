using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DrawDelaunay))]
public class DrawDelaunayEditor : Editor
{
    DrawDelaunay eTarget;
    private void OnEnable()
    {
        eTarget = (DrawDelaunay)target;
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Generate"))
            eTarget.Compute();
    }
    private void OnSceneGUI()
    {
        for (int i = 0; i < eTarget.Vertices.Count && i < 10000; i++)
        {
            Handles.Label(eTarget.Vertices[i] + Vector3.up * 0.5f, i.ToString().ToUpper());
            eTarget.Vertices[i] = Handles.DoPositionHandle(eTarget.Vertices[i], Quaternion.identity);
        }
        if (eTarget.Triangles == null) return;
        for (int i = 0; i < eTarget.Triangles.Count && i < 10000; i++)
        {
            Triangle _t = eTarget.Triangles[i];
            if (_t == null) continue;
            Vector3 _center = _t.GetCenterTriangle();
            //Handles.DrawWireDisc(_center, Vector3.up, Vector3.Distance(_center, _t.A));
            Handles.DrawLine(_t.A, _t.B);
            Handles.DrawLine(_t.B, _t.C);
            Handles.DrawLine(_t.C, _t.A);
        }
    }
}
