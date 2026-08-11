using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProceduralCastle : MonoBehaviour
{
    [Header("Layout Settings")]
    [SerializeField] private float castleRadius = 10.0f;
    [SerializeField] private int wallSides = 6; // 4 = Square, 6 = Hexagonal, 8 = Octagonal layout

    [Header("Wall Settings")]
    [SerializeField] private float wallHeight = 4.0f;
    [SerializeField] private float wallThickness = 0.8f;

    [Header("Corner Towers")]
    [SerializeField] private float cornerTowerRadius = 1.8f;
    [SerializeField] private float cornerTowerHeight = 6.5f;
    [SerializeField] private float cornerRoofHeight = 2.5f;

    [Header("Central Keep (Main Tower)")]
    [SerializeField] private bool includeKeep = true;
    [SerializeField] private float keepRadius = 3.5f;
    [SerializeField] private float keepHeight = 10.0f;
    [SerializeField] private float keepRoofHeight = 3.5f;

    [Header("Materials")]
    [SerializeField] private Material wallMaterial;
    [SerializeField] private Material roofMaterial;

    [ContextMenu("Generate Castle")]
    public void GenerateCastle()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        MeshRenderer mr = GetComponent<MeshRenderer>();

        // Assign materials to the renderer array (Element 0 = Walls, Element 1 = Roofs)
        mr.sharedMaterials = new Material[] { wallMaterial, roofMaterial };

        Mesh castleMesh = new Mesh { name = "Procedural Castle Complex" };
        List<Vector3> verts = new List<Vector3>();

        // Split triangles into two separate submeshes
        List<int> wallTris = new List<int>();
        List<int> roofTris = new List<int>();

        // 1. GENERATE CORNER TOWERS
        List<Vector3> cornerPositions = new List<Vector3>();
        for (int i = 0; i < wallSides; i++)
        {
            float angle = (i / (float)wallSides) * Mathf.PI * 2f;
            Vector3 pos = new Vector3(Mathf.Cos(angle) * castleRadius, 0, Mathf.Sin(angle) * castleRadius);
            cornerPositions.Add(pos);

            // Skip corner tower at index 0 for a gate entrance
            if (i != 0)
            {
                BuildTower(pos, cornerTowerRadius, cornerTowerHeight, cornerRoofHeight, 8, verts, wallTris, roofTris);
            }
        }

        // 2. GENERATE CURTAIN WALLS
        for (int i = 0; i < wallSides; i++)
        {
            if (i == 0) continue; // Gate entrance gap

            Vector3 p1 = cornerPositions[i];
            Vector3 p2 = cornerPositions[(i + 1) % wallSides];
            BuildWallSegment(p1, p2, wallHeight, wallThickness, verts, wallTris);
        }

        // 3. GENERATE CENTRAL KEEP
        if (includeKeep)
        {
            BuildTower(Vector3.zero, keepRadius, keepHeight, keepRoofHeight, 10, verts, wallTris, roofTris);
        }

        // Apply geometry
        castleMesh.SetVertices(verts);

        // Define 2 submeshes for Unity to assign two materials
        castleMesh.subMeshCount = 2;
        castleMesh.SetTriangles(wallTris, 0); // Submesh 0 -> Uses wallMaterial
        castleMesh.SetTriangles(roofTris, 1); // Submesh 1 -> Uses roofMaterial

        castleMesh.RecalculateBounds();
        castleMesh.RecalculateNormals();

        mf.mesh = castleMesh;
    }

    private void BuildTower(Vector3 center, float radius, float height, float roofHeight, int segments,
                            List<Vector3> verts, List<int> wallTris, List<int> roofTris)
    {
        int baseIndex = verts.Count;

        // --- WALL CYLINDER (Submesh 0) ---
        for (int i = 0; i <= segments; i++)
        {
            float angle = (i / (float)segments) * Mathf.PI * 2f;
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);

            Vector3 offset = new Vector3(cos * radius, 0, sin * radius);
            verts.Add(center + offset);
            verts.Add(center + offset + Vector3.up * height);
        }

        for (int i = 0; i < segments; i++)
        {
            int b1 = baseIndex + (i * 2);
            int t1 = b1 + 1;
            int b2 = baseIndex + ((i + 1) * 2);
            int t2 = b2 + 1;

            wallTris.AddRange(new[] { b1, t1, b2, b2, t1, t2 });
        }

        // --- ROOF CONE (Submesh 1) ---
        int roofIndex = verts.Count;
        Vector3 roofTip = center + Vector3.up * (height + roofHeight);
        float roofOverhang = radius * 1.15f;

        for (int i = 0; i <= segments; i++)
        {
            float angle = (i / (float)segments) * Mathf.PI * 2f;
            Vector3 offset = new Vector3(Mathf.Cos(angle) * roofOverhang, 0, Mathf.Sin(angle) * roofOverhang);

            verts.Add(center + offset + Vector3.up * height);
            verts.Add(roofTip);
        }

        for (int i = 0; i < segments; i++)
        {
            int b1 = roofIndex + (i * 2);
            int t1 = b1 + 1;
            int b2 = roofIndex + ((i + 1) * 2);

            roofTris.AddRange(new[] { b1, t1, b2 });
        }
    }

    private void BuildWallSegment(Vector3 start, Vector3 end, float height, float thickness,
                                 List<Vector3> verts, List<int> wallTris)
    {
        Vector3 dir = (end - start).normalized;
        Vector3 normal = Vector3.Cross(dir, Vector3.up).normalized * (thickness * 0.5f);

        int baseIndex = verts.Count;

        // Front Face
        verts.Add(start + normal);
        verts.Add(start + normal + Vector3.up * height);
        verts.Add(end + normal);
        verts.Add(end + normal + Vector3.up * height);

        // Back Face
        verts.Add(start - normal);
        verts.Add(start - normal + Vector3.up * height);
        verts.Add(end - normal);
        verts.Add(end - normal + Vector3.up * height);

        // Top Face
        verts.Add(start + normal + Vector3.up * height);
        verts.Add(start - normal + Vector3.up * height);
        verts.Add(end + normal + Vector3.up * height);
        verts.Add(end - normal + Vector3.up * height);

        // Add to Wall Submesh
        wallTris.AddRange(new[] { baseIndex, baseIndex + 1, baseIndex + 2, baseIndex + 2, baseIndex + 1, baseIndex + 3 });
        wallTris.AddRange(new[] { baseIndex + 6, baseIndex + 5, baseIndex + 4, baseIndex + 7, baseIndex + 5, baseIndex + 6 });
        wallTris.AddRange(new[] { baseIndex + 8, baseIndex + 9, baseIndex + 10, baseIndex + 10, baseIndex + 9, baseIndex + 11 });
    }
}