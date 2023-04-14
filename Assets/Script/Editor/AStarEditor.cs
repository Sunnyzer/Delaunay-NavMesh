using UnityEngine;
using UnityEditor;
using static Codice.CM.Common.CmCallContext;

[CustomEditor(typeof(AStar))]
public class AStarEditor : Editor
{
    AStar eTarget = null;
    private void OnEnable()
    {
        eTarget = (AStar)target;
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        if (GUILayout.Button("Generate Path"))
        {
            eTarget.ComputePath();
            //eTarget.current = 0;
        }
    }
    private void OnSceneGUI()
    {
        eTarget.current = 0;
        eTarget.ComputePath();

        Handles.color = Color.blue;
        if(eTarget.path.Count == 1)
        {
            Handles.DrawWireCube(eTarget.path[0], Vector3.one * 0.25f);
        }
        for (int i = 0; i < eTarget.path.Count - 1; i++)
        {
            Handles.DrawWireCube(eTarget.path[i], Vector3.one * 0.25f);
            Handles.DrawWireCube(eTarget.path[i + 1], Vector3.one * 0.25f);
            Handles.DrawLine(eTarget.path[i] + Vector3.up, eTarget.path[i + 1] + Vector3.up);
        }
        Handles.color = Color.black;
        for (int i = 0; i < eTarget.pathNode.Count - 1; i++)
        {
            Handles.DrawWireCube(eTarget.pathNode[i].position, Vector3.one * 0.25f);
            Handles.DrawWireCube(eTarget.pathNode[i + 1].position, Vector3.one * 0.25f);
            Handles.DrawLine(eTarget.pathNode[i].position + Vector3.up, eTarget.pathNode[i + 1].position + Vector3.up);
        }
        //if (eTarget.current < eTarget.path.Count)
        //{
        //    eTarget.transform.position = Vector3.MoveTowards(eTarget.transform.position, eTarget.path[eTarget.current], Time.deltaTime);
        //    if (Vector3.Distance(eTarget.transform.position, eTarget.path[eTarget.current]) < 0.05f)
        //        ++eTarget.current;
        //}
        //if (eTarget.path.Count != 0)
         //   eTarget.transform.position = Vector3.MoveTowards(eTarget.transform.position, eTarget.path[0], Time.deltaTime);
    }
}
