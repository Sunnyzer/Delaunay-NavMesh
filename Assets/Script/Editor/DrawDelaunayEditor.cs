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
        for (int i = 0; i < eTarget.Vertices.Count; i++)
        {
            Handles.Label(eTarget.Vertices[i] + Vector3.up * 0.1f, i.ToString());
            eTarget.Vertices[i] = Handles.DoPositionHandle(eTarget.Vertices[i], Quaternion.identity);
        }
        if (eTarget.Triangles == null) return;
        for (int i = 0; i < eTarget.Triangles.Count; i++)
        {
            if (eTarget.Triangles[i] == null) continue;
            Vector3 _center = eTarget.Triangles[i].GetCenterTriangle();
            Handles.DrawWireDisc(_center, Vector3.up, Vector3.Distance(_center, eTarget.Triangles[i].A));
        }
    }
}
