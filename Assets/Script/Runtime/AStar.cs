using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Analytics;

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
    public List<Node> pathNode = new List<Node>();
    public List<Vector3> path = new List<Vector3>();
    List<NodeData> openList = new List<NodeData>();
    List<Node> closeList = new List<Node>();
    public int current = 0;
    public Node ClosestNode(Vector3 _start)
    {
        DrawDelaunay _drawDelaunay = DrawDelaunay.Instance;
        if (!_drawDelaunay)
            _drawDelaunay = FindObjectOfType<DrawDelaunay>();
        if (_drawDelaunay.Path.Count == 0) return null;
        int i = 0;
        for (; i < _drawDelaunay.Triangles.Count; i++)
            if (_drawDelaunay.Triangles[i].IsPointInTriangle(_start))
                break;
        if(i >= _drawDelaunay.Path.Count)
            return null;
        return _drawDelaunay.Path[i];
    }
    public List<Node> ComputePath()
    {
        path.Clear();
        openList.Clear();
        pathNode.Clear();
        closeList.Clear();
        List<Node> _pathNode = new List<Node>(); 
        Node _start = ClosestNode(start.position);
        Node _goal = ClosestNode(goal.position);
        if (_start == null || _goal == null) return new List<Node>();
        NodeData _currentNode = new NodeData(_start, 0, Vector3.Distance(_start, _goal), null, null);
        List<Node> navMeshNode = FindObjectOfType<DrawDelaunay>().Path;
        int max = 0;
        while (_currentNode.currentNode != _goal && max < 200)
        {
            if (_currentNode.currentNode.neighbors == null) break; 
            foreach (var _neighbor in _currentNode.currentNode.neighbors)
            {
                Node _node = navMeshNode[_neighbor.Value];
                if (closeList.Contains(_node)) continue;
                float g = Vector3.Distance(_node, _start);
                float h = Vector3.Distance(_node, _goal);
                openList.Add(new NodeData(_node, g, h, _currentNode, _neighbor.Key));
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
                break;

            openList.Remove(_nextNode);
            _currentNode = _nextNode;
            closeList.Add(_currentNode.currentNode);
            max++;
        }
        List<Vector3> _path = new List<Vector3>();
        Edge _edgePrevious = null;
        for (NodeData _actual = _currentNode; _actual != null; _actual = _actual.previousNode)
        {
            if (!_actual.edge) break;
            float gA = Vector3.Distance(_actual.edge.A, _start);
            float hA = Vector3.Distance(_actual.edge.A, _goal);
            float gB = Vector3.Distance(_actual.edge.B, _start);
            float hB = Vector3.Distance(_actual.edge.B, _goal);
            //if (_edgePrevious)
            //    if (_edgePrevious.ContainsPoint(_actual.edge.A) || _edgePrevious.ContainsPoint(_actual.edge.B))
            //        continue;
            if (gA + hA <= gB + hB)
            {
                bool _isSame = false;
                for (int i = 0; i < _path.Count; i++)
                {
                    if (_path[i] == _actual.edge.A)
                        _isSame = true;
                }
                if(!_isSame)
                    _path.Insert(0, _actual.edge.A);
            }
            else
            {
                bool _isSame = false;
                for (int i = 0; i < _path.Count; i++)
                {
                    if (_path[i] == _actual.edge.A)
                        _isSame = true;
                }
                if (!_isSame)
                    _path.Insert(0, _actual.edge.B);
            }
            _edgePrevious = _actual.edge;
            Debug.DrawLine(_edgePrevious.A + Vector3.up * 2, _edgePrevious.B + Vector3.up * 2, Color.magenta, 0);
            _pathNode.Insert(0, _actual.currentNode);
        }
        _path.Insert(0, start.position);
        _path.Add(goal.position);

        //for (int i = 1; i < _path.Count - 2;)
        //{
        //    bool _hit = Physics.Linecast(_path[i], _path[i + 2]);
        //    if (!_hit)
        //    {
        //        _path.RemoveAt(i + 1);
        //        continue;
        //    }
        //    i++;
        //}
        path = _path;
        pathNode = _pathNode;
        return _pathNode;
    }
}
