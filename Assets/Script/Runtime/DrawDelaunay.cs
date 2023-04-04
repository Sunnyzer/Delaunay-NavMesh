using System.Collections.Generic;
using UnityEngine;

public class DrawDelaunay : MonoBehaviour
{
    [SerializeField] Delaunay delaunay = new Delaunay();
    [SerializeField] List<Vector3> vertices = new List<Vector3>();
    public List<Vector3> Vertices => vertices;
    public List<Triangle> Triangles { get; set; }
    public void Compute()
    {
        Triangles = delaunay.ComputeDelaunay(vertices);
        vertices = delaunay.Vertices;
    }
    
    private void OnDrawGizmos()
    {
        delaunay.DrawGizmo();
    }
}
