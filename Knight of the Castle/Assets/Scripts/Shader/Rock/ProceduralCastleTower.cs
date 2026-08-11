using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProceduralCastleTower : MonoBehaviour
{
    [Header("Tower Dimensions")]
    [SerializeField] private float towerHeight = 6.0f;
    [SerializeField] private float towerRadius = 2.0f;
    [SerializeField] private int segments = 8;

    [Header("Roof Settings")]
    [SerializeField] private float roofHeight = 2.5f;
    [SerializeField] private float roofOverhang = 0.4f;

    [Header("Material")]
    [SerializeField] private Material towerMaterial;

    [ContextMenu("Generate Tower Mesh")]
    public void GenerateTower()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        MeshRenderer mr = GetComponent<MeshRenderer>();

        if (towerMaterial != null) mr.sharedMaterial = towerMaterial;

        Mesh mesh = new Mesh { name = "Procedural Castle Tower" };
        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();
        List<Color> colors = new List<Color>();

        // 1. TOWER CYLINDER (Stone Wall -> Red = 0)
        for (int i = 0; i <= segments; i++)
        {
            float angle = (i / (float)segments) * Mathf.PI * 2f;
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);

            verts.Add(new Vector3(cos * towerRadius, 0, sin * towerRadius));
            colors.Add(new Color(0, 0, 0, 1));

            verts.Add(new Vector3(cos * towerRadius, towerHeight, sin * towerRadius));
            colors.Add(new Color(0, 0, 0, 1));
        }

        for (int i = 0; i < segments; i++)
        {
            int b1 = i * 2, t1 = b1 + 1;
            int b2 = (i + 1) * 2, t2 = b2 + 1;

            tris.AddRange(new[] { b1, t1, b2, b2, t1, t2 });
        }

        // 2. CONICAL ROOF (Roof Tiles -> Red = 1)
        int roofStart = verts.Count;
        Vector3 roofTip = new Vector3(0, towerHeight + roofHeight, 0);
        float roofRad = towerRadius + roofOverhang;

        for (int i = 0; i <= segments; i++)
        {
            float angle = (i / (float)segments) * Mathf.PI * 2f;
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);

            verts.Add(new Vector3(cos * roofRad, towerHeight, sin * roofRad));
            colors.Add(new Color(1, 0, 0, 1)); // Tagged as Accent/Roof

            verts.Add(roofTip);
            colors.Add(new Color(1, 0, 0, 1));
        }

        for (int i = 0; i < segments; i++)
        {
            int b1 = roofStart + (i * 2);
            int t1 = b1 + 1;
            int b2 = roofStart + ((i + 1) * 2);

            tris.AddRange(new[] { b1, t1, b2 });
        }

        mesh.SetVertices(verts);
        mesh.SetTriangles(tris, 0);
        mesh.SetColors(colors);
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        mf.mesh = mesh;
    }
}