using System.Collections.Generic;
using UnityEngine;

public class DrawDelaunay : MonoBehaviour
{
    Delaunay delaunay = new Delaunay();
    [SerializeField] List<Vector3> vertices = new List<Vector3>();
    public List<Vector3> Vertices => vertices;
    public List<Triangle> Triangles { get; set; }
    public void Compute()
    {
        Geometry _geometry = delaunay.ComputeDelaunay(vertices);
        Triangles = _geometry.Triangles;
        vertices = _geometry.Vertices;
    }
}
