using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProceduralLowPolyRock : MonoBehaviour
{
    [Header("Rock Settings")]
    [SerializeField] private float radius = 1.0f;
    [SerializeField] private float deformationStrength = 0.35f;
    [SerializeField] private int seed = 42;

    [Header("Material")]
    [SerializeField] private Material rockMaterial;

    [ContextMenu("Generate Rock Mesh")]
    public void GenerateRock()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        MeshRenderer mr = GetComponent<MeshRenderer>();

        if (rockMaterial != null) mr.sharedMaterial = rockMaterial;

        Random.InitState(seed);
        Mesh mesh = new Mesh { name = "Procedural LowPoly Rock" };

        // Icosahedron Base Vertices
        float t = (1.0f + Mathf.Sqrt(5.0f)) / 2.0f;
        List<Vector3> baseVerts = new List<Vector3>
        {
            new Vector3(-1,  t,  0).normalized, new Vector3( 1,  t,  0).normalized,
            new Vector3(-1, -t,  0).normalized, new Vector3( 1, -t,  0).normalized,
            new Vector3( 0, -1,  t).normalized, new Vector3( 0,  1,  t).normalized,
            new Vector3( 0, -1, -t).normalized, new Vector3( 0,  1, -t).normalized,
            new Vector3( t,  0, -1).normalized, new Vector3( t,  0,  1).normalized,
            new Vector3(-t,  0, -1).normalized, new Vector3(-t,  0,  1).normalized
        };

        int[] baseTris = new int[]
        {
            0,11,5,  0,5,1,   0,1,7,   0,7,10,  0,10,11,
            1,5,9,   5,11,4,  11,10,2, 10,7,6,  7,1,8,
            3,9,4,   3,4,2,   3,2,6,   3,6,8,   3,8,9,
            4,9,5,   2,4,11,  6,2,10,  8,6,7,   9,8,1
        };

        List<Vector3> flatVerts = new List<Vector3>();
        List<int> flatTris = new List<int>();
        List<Color> colors = new List<Color>();

        // Apply low-poly flat shading noise
        for (int i = 0; i < baseTris.Length; i += 3)
        {
            Vector3 v1 = DeformVertex(baseVerts[baseTris[i]]);
            Vector3 v2 = DeformVertex(baseVerts[baseTris[i + 1]]);
            Vector3 v3 = DeformVertex(baseVerts[baseTris[i + 2]]);

            int currIndex = flatVerts.Count;

            flatVerts.Add(v1);
            flatVerts.Add(v2);
            flatVerts.Add(v3);

            // Assign Red vertex channel based on vertical orientation (for top-moss/shading)
            colors.Add(new Color(v1.y > 0.4f ? 0.8f : 0f, 0, 0, 1));
            colors.Add(new Color(v2.y > 0.4f ? 0.8f : 0f, 0, 0, 1));
            colors.Add(new Color(v3.y > 0.4f ? 0.8f : 0f, 0, 0, 1));

            flatTris.Add(currIndex);
            flatTris.Add(currIndex + 1);
            flatTris.Add(currIndex + 2);
        }

        mesh.SetVertices(flatVerts);
        mesh.SetTriangles(flatTris, 0);
        mesh.SetColors(colors);
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        mf.mesh = mesh;
    }

    private Vector3 DeformVertex(Vector3 v)
    {
        float noise = Mathf.PerlinNoise(v.x * 2.5f + seed, v.z * 2.5f + seed);
        float scale = radius + (noise - 0.5f) * deformationStrength;
        return v * scale;
    }
}