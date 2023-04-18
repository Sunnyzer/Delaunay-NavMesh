using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NavMeshData", order = 0)]
public class NavMeshData : ScriptableObject
{
    public List<Triangle> triangles = new List<Triangle>();
    public List<Node> nodes = new List<Node>();
}
