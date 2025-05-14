using UnityEngine;
using UnityEngine.Splines;
using System.Collections.Generic;
using Unity.Mathematics;


[RequireComponent(typeof(SplineContainer), typeof(MeshFilter), typeof(MeshRenderer))]
public class CustomSplineExtruder : MonoBehaviour
{
    public float width = 1f;   // Yatay ölçekleme (X ekseni)
    public float height = 1f;  // Dikey ölçekleme (Y ekseni)
    public int segmentsPerUnit = 10;

    private Mesh mesh;
    private Spline spline;

    public void Extrude()
    {
        var container = GetComponent<SplineContainer>();
        spline = container.Spline;
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        GenerateMesh();
    }

    void GenerateMesh()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        var length = SplineUtility.CalculateLength(spline, float4x4.identity);
        int segments = Mathf.CeilToInt(length * segmentsPerUnit);

        for (int i = 0; i <= segments; i++)
        {
            float t = i / (float)segments;
            float3 pos, tangent;
            SplineUtility.Evaluate(spline, t, out pos, out tangent, out float3 up);


            Vector3 right = Vector3.Cross(up, tangent).normalized;

            Vector3 offsetRight = right * width * 0.5f;
            Vector3 offsetUp = up * height * 0.5f;

            Vector3 position = pos;
            vertices.Add(position - offsetRight - offsetUp);
            vertices.Add(position + offsetRight - offsetUp);
            vertices.Add(position + offsetRight + offsetUp);
            vertices.Add(position - offsetRight + offsetUp);

            if (i < segments)
            {
                int idx = i * 4;
                // İlk üçgen
                triangles.Add(idx);
                triangles.Add(idx + 1);
                triangles.Add(idx + 5);
                // İkinci üçgen
                triangles.Add(idx);
                triangles.Add(idx + 5);
                triangles.Add(idx + 4);
                // Üçüncü üçgen
                triangles.Add(idx + 1);
                triangles.Add(idx + 2);
                triangles.Add(idx + 6);
                // Dördüncü üçgen
                triangles.Add(idx + 1);
                triangles.Add(idx + 6);
                triangles.Add(idx + 5);
                // Beşinci üçgen
                triangles.Add(idx + 2);
                triangles.Add(idx + 3);
                triangles.Add(idx + 7);
                // Altıncı üçgen
                triangles.Add(idx + 2);
                triangles.Add(idx + 7);
                triangles.Add(idx + 6);
                // Yedinci üçgen
                triangles.Add(idx + 3);
                triangles.Add(idx);
                triangles.Add(idx + 4);
                // Sekizinci üçgen
                triangles.Add(idx + 3);
                triangles.Add(idx + 4);
                triangles.Add(idx + 7);
            }
        }

        mesh.Clear();
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
    }
}
