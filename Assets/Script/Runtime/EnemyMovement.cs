using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [SerializeField] AStar astar;
    [SerializeField] List<Vector3> path;
    [SerializeField] Transform goal;
    [SerializeField] float moveSpeed = 1;
    private void Update()
    {
        path = astar.ComputePath(goal);
        if(path.Count > 0)
        {
            transform.position = Vector3.MoveTowards(transform.position, path[0], Time.deltaTime * moveSpeed);
            return;
        }
    }
    private void OnDrawGizmos()
    {
        int _count = path.Count;
        Gizmos.color = Color.blue;
        for (int i = 0; i < _count - 1; i++)
        {
            Gizmos.DrawLine(path[i], path[i + 1]);
        }
    }
}
