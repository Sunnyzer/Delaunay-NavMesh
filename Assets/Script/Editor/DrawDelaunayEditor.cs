using System;
using TMPro;
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
        {
            GeneratePoint();
            eTarget.Compute();
        }
            
        if (GUILayout.Button("GeneratePoint"))
            GeneratePoint();
    }
    private void OnSceneGUI()
    {
        //GeneratePoint();
        //eTarget.Compute();
        Handles.color = Color.white;
        for (int i = 0; i < eTarget.Vertices.Count && i < 10000; i++)
        {
            Handles.Label(eTarget.Vertices[i] + Vector3.up * 0.5f, i.ToString().ToUpper());
            if(Vector3.Distance(eTarget.Vertices[i], Camera.current.transform.position) < 5)
                eTarget.Vertices[i] = Handles.DoPositionHandle(eTarget.Vertices[i], Quaternion.identity);
        }
        Collider[] _colliders = Physics.OverlapBox(eTarget.transform.position, eTarget.Extends / 2);
        Handles.color = Color.red;
        for (int i = 0; i < _colliders.Length; i++)
        {
            Handles.DrawWireCube(_colliders[i].transform.position, _colliders[i].bounds.extents * 2);
        }
        Handles.color = Color.black;
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
    public void GeneratePoint()
    {
        eTarget.Vertices.Clear();
        Vector3 _extends = eTarget.Extends / 2; 
        AddPoint(eTarget.transform.position + _extends);
        AddPoint(eTarget.transform.position - _extends);
        AddPoint(eTarget.transform.position - new Vector3(_extends.x, 0, -_extends.z));
        AddPoint(eTarget.transform.position - new Vector3(-_extends.x, 0, _extends.z));
        Collider[] _colliders = Physics.OverlapBox(eTarget.transform.position, _extends);
        for (int i = 0; i < _colliders.Length; i++)
        {
            Handles.color = Color.white;
            Bounds _bounds = _colliders[i].bounds;

            Handles.color = Color.red;
            Vector3 _extendsWithoutY = new Vector3(_bounds.extents.x, 0, _bounds.extents.z);
            Vector3 _extendsOpposite = new Vector3(_bounds.extents.x, 0, -_bounds.extents.z);

            Vector3 _boundPExtends = _bounds.center + _extendsWithoutY;
            Vector3 _boundMExtends = _bounds.center - _extendsWithoutY;
            Vector3 _boundPExtendsOpposite = _bounds.center + _extendsOpposite;
            Vector3 _boundMExtendsOpposite = _bounds.center - _extendsOpposite;

            Vector3 _offset = new Vector3(1, 0, 1) * 0.2f;
            Vector3 _offsetOpposite = new Vector3(1, 0, -1) * 0.2f;

            Vector3[] _allPoint = new Vector3[]
            { 
                //_boundPExtends - _offset,
                //_boundMExtends + _offset,
                //_boundMExtendsOpposite + _offsetOpposite,
                //_boundPExtendsOpposite - _offsetOpposite,
                _boundMExtendsOpposite - _offsetOpposite,
                _boundPExtendsOpposite + _offsetOpposite,
                _boundMExtends - _offset,
                _boundPExtends + _offset,
            };
            LayerMask _layer = eTarget.Layer;
            for (int j = 0; j < _allPoint.Length; ++j)
            {
                Vector3 _point = _allPoint[j];
                AddPoint(_point);
            }
        }
    }
    public void AddPoint(Vector3 _point)
    {
        bool _hit = Physics.Raycast(_point + Vector3.up * 10000, Vector3.down, out RaycastHit _rayHit, Mathf.Infinity, eTarget.Layer);
        if (_hit)
        {
            Vector3 _position = new Vector3((float)Math.Round(_rayHit.point.x, 2), (float)Math.Round(_rayHit.point.y, 2), (float)Math.Round(_rayHit.point.z, 2));
            eTarget.Vertices.Add(_position);
        }
    }
}
