using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

public class NodeData
{
    public NodeData(Node _currentNode, float _g, float _h, NodeData _previousNode, Edge edge)
    {
        currentNode = _currentNode;
        g = _g;
        h = _h;
        previousNode = _previousNode;
        this.edge = edge;
    }
    public float g = float.MaxValue/2;
    public float h = float.MaxValue/2;
    public float FCost => g + h;
    public Node currentNode = null;
    public NodeData previousNode = null;
    public Edge edge = null;
}

public class AStar : MonoBehaviour
{
    [SerializeField] Transform start;
    [SerializeField] Transform goal;
    [SerializeField] LayerMask obstacleLayer;
    [SerializeField] float avoidance = 0.5f;
    public List<Node> pathNode = new List<Node>();
    public List<Vector3> path = new List<Vector3>();
    List<NodeData> openList = new List<NodeData>();
    List<Node> closeList = new List<Node>();
    public int current = 0;
    public Node ClosestNode(Vector3 _start)
    {
        NavMesh _drawDelaunay = NavMesh.Instance;
        if (!_drawDelaunay)
            _drawDelaunay = FindObjectOfType<NavMesh>();
        if (_drawDelaunay.Path.Count == 0) return null;
        int i = 0;
        for (; i < _drawDelaunay.Triangles.Count; i++)
            if (_drawDelaunay.Triangles[i].IsPointInTriangle(_start))
                break;
        if(i >= _drawDelaunay.Path.Count)
            return null;
        return _drawDelaunay.Path[i];
    }
    public List<Vector3> ComputePath(Transform _goalT)
    {
        if(_goalT)
            goal = _goalT;
        path.Clear();
        openList.Clear();
        pathNode.Clear();
        closeList.Clear();
        if (!Physics.CheckCapsule(start.position, goal.position, avoidance, obstacleLayer))
        {
            path.Add(goal.position);
            return path;
        }
        List<Node> _pathNode = new List<Node>(); 
        Node _start = ClosestNode(start.position);
        Node _goal = ClosestNode(goal.position);

        if (_start == null || _goal == null) return path;

        NodeData _currentNode = new NodeData(_start, 0, Vector3.Distance(_start, _goal), null, null);
        List<Node> navMeshNode = FindObjectOfType<NavMesh>().Path;
        int max = 0;
        while (_currentNode.currentNode != _goal && max < 500)
        {
            int j = 0;
            foreach (var _neighbor in _currentNode.currentNode.neighborsIndex)
            {
                Node _node = navMeshNode[_neighbor];
                if (closeList.Contains(_node))
                {
                    j++;
                    continue;
                }
                float g = 0;
                float h= 0;
                NodeData _nodeData = openList.Find(n => n.currentNode == _node);
                g = Vector3.Distance(_node, start.position);
                h = Vector3.Distance(_node, goal.position);
                if(_nodeData != null)
                    if(_nodeData.FCost <= g + h)
                        _nodeData.previousNode = _currentNode;
                openList.Add(new NodeData(_node, g, h, _currentNode, _currentNode.currentNode.neighborsEdge[j]));
                j++;
            }
            float fCost = float.MaxValue;
            NodeData _nextNode = null;
            for (int i = 0; i < openList.Count; i++)
            {
                NodeData _nodeData = openList[i];
                if(_nodeData.FCost < fCost)
                {
                    _nextNode = _nodeData;
                    fCost = _nodeData.FCost;
                }
            }
            if (_nextNode == null)
                continue;

            openList.Remove(_nextNode);
            _currentNode = _nextNode;
            closeList.Add(_currentNode.currentNode);
            max++;
        }
        List<Vector3> _path = new List<Vector3>();
        for (NodeData _actual = _currentNode; _actual != null; _actual = _actual.previousNode)
        {
            if(!_actual.edge) break;
            float gA = Vector3.Distance(_actual.edge.A, start.position);
            float hA = Vector3.Distance(_actual.edge.A, goal.position);
            float gB = Vector3.Distance(_actual.edge.B, start.position);
            float hB = Vector3.Distance(_actual.edge.B, goal.position);
            if (gA + hA <= gB + hB)
            {
                Vector3 _avoidance = _actual.edge.A + (_actual.edge.B - _actual.edge.A).normalized * avoidance;
                bool _isSame = false;
                for (int i = 0; i < _path.Count; i++)
                {
                    if (_path[i] == _avoidance)
                        _isSame = true;
                }
                if(!_isSame)
                {

                    _path.Insert(0, _avoidance);
                }
            }
            else
            {
                Vector3 _avoidance = _actual.edge.B + (_actual.edge.A - _actual.edge.B).normalized * avoidance;
                bool _isSame = false;
                for (int i = 0; i < _path.Count; i++)
                {
                    if (_path[i] == _avoidance)
                        _isSame = true;
                }
                if (!_isSame)
                    _path.Insert(0, _avoidance);
            }
            _pathNode.Insert(0, _actual.currentNode);
        }
        _path.Insert(0, start.position);
        _path.Add(goal.position);
        for (int i = 0; i < _path.Count - 2; )
        {
            if (!Physics.CheckCapsule(_path[i], _path[i + 2], avoidance, obstacleLayer))
            {
                _path.RemoveAt(i + 1);
                continue;
            }
            i++;
        }
        if (_path.Count > 2 && !Physics.CheckCapsule(_path[0], _path[2], avoidance, obstacleLayer))
        {
            _path.RemoveAt(1);
        }
        _path.RemoveAt(0);
        path = _path;
        pathNode = _pathNode;
        return path;
    }
}
