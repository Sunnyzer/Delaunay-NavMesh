using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class DrawDelaunay : MonoBehaviour
{
    [SerializeField] Delaunay delaunay = new Delaunay();
    [SerializeField] List<Vector3> vertices = new List<Vector3>();
    [SerializeField] Vector3 extends = Vector3.one;
    [SerializeField] bool navMeshVolumeDebug = true;
    [SerializeField] LayerMask layerNav;
    public List<Vector3> Vertices => vertices;
    public List<Triangle> Triangles { get; set; }
    public Vector3 Extends => extends;
    public LayerMask Layer => layerNav;


    private void FixedUpdate()
    {
        
    }
    public void Compute()
    {
        NavMeshTriangulation navMeshTriangulation = NavMesh.CalculateTriangulation();
        //vertices = navMeshTriangulation.vertices.ToList();
        //vertices.FindAll(v => v );
        Geometry _geometry = delaunay.ComputeDelaunay(vertices);
        Triangles = _geometry.Triangles;
        vertices = _geometry.Vertices;
    }
    private void OnDrawGizmos()
    {
        if (!navMeshVolumeDebug) return;
        Gizmos.color = Color.green - new Color(0,0,0, 0.5f);
        Gizmos.DrawCube(transform.position, extends);
    }
}
