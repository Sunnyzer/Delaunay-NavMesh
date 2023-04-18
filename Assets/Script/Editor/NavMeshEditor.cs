using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NavMesh))]
public class NavMeshEditor : Editor
{
    NavMesh eTarget;
    private void OnEnable()
    {
        eTarget = (NavMesh)target;
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Generate"))
        {
            GeneratePoint();
            eTarget.Compute();
        }
    }
    private void OnSceneGUI()
    {
        Handles.color = Color.white;
        List<Vector3> _vertices = eTarget.Vertices;
        int _count = _vertices.Count;
        int i = 0;
        for (; i < _count; i++)
        {
            Vector3 _v = _vertices[i];
            Handles.Label(_v + Vector3.up * 0.5f, i.ToString().ToUpper());
            if (Vector3.Distance(_v, Camera.current.transform.position) < 5)
                eTarget.Vertices[i] = Handles.DoPositionHandle(_v, Quaternion.identity);
        }
        i = 0;
        Collider[] _colliders = Physics.OverlapBox(eTarget.transform.position, eTarget.Extends / 2);
        Handles.color = Color.red;
        List<Node> _nodes = eTarget.Path;
        _count = _nodes.Count;
        for (; i < _count; i++)
        {
            Node _node = _nodes[i];
            foreach (var neighbor in _node.neighborsIndex)
                Handles.DrawLine(_node.position, _nodes[neighbor]);
        }
        List<Triangle> _triangles = eTarget.Triangles; 
        if (_triangles == null) return;
        _count = _triangles.Count;
        Handles.color = Color.black;
        i = 0;
        for (; i < _count; i++)
        {
            Triangle _t = _triangles[i];
            if (_t == null) continue;
            float _ratio = 0.25f * i;
            _ratio = 0;
            Handles.DrawLine(_t.A + Vector3.up * _ratio, _t.B + Vector3.up * _ratio, _ratio);
            Handles.DrawLine(_t.B + Vector3.up * _ratio, _t.C + Vector3.up * _ratio, _ratio);
            Handles.DrawLine(_t.C + Vector3.up * _ratio, _t.A + Vector3.up * _ratio, _ratio);
        }
    }
    public void GeneratePoint()
    {
        eTarget.Vertices.Clear();
        Vector3 _extends = eTarget.Extends / 2;
        Vector3 _position = eTarget.transform.position;
        AddPoint(_position + _extends);
        AddPoint(_position - _extends);
        AddPoint(_position - new Vector3(_extends.x, 0, -_extends.z));
        AddPoint(_position - new Vector3(-_extends.x, 0, _extends.z));
        Collider[] _colliders = Physics.OverlapBox(_position, _extends);
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
            for (int j = 0; j < _allPoint.Length; ++j)
            {
                Vector3 _point = _allPoint[j];
                AddPoint(_point);
            }
        }
    }
    public void AddPoint(Vector3 _point)
    {
        bool _hit = Physics.Raycast(_point + Vector3.up * 10000, Vector3.down, out RaycastHit _rayHit, Mathf.Infinity, eTarget.ObstacleLayer);
        if (_hit)
        {
            Vector3 _position = new Vector3((float)Math.Round(_rayHit.point.x, 2), (float)Math.Round(_rayHit.point.y, 2), (float)Math.Round(_rayHit.point.z, 2));
            eTarget.Vertices.Add(_position);
        }
    }
}
