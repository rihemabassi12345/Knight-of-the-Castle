using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProceduralLowPolyTree : MonoBehaviour
{
    [Header("Trunk Settings")]
    [SerializeField] private float trunkHeight = 2.0f;
    [SerializeField] private float trunkRadiusBottom = 0.4f;
    [SerializeField] private float trunkRadiusTop = 0.2f;
    [SerializeField] private int trunkSegments = 6; // Low poly count

    [Header("Foliage Layers Settings")]
    [SerializeField] private int foliageLayers = 3;
    [SerializeField] private float foliageStartHeight = 1.2f;
    [SerializeField] private float layerHeight = 1.5f;
    [SerializeField] private float baseFoliageRadius = 1.8f;
    [SerializeField] private float radiusShrinkPerLayer = 0.4f;
    [SerializeField] private int foliageSegments = 7; // Low poly faces per layer

    [Header("Material Reference")]
    [SerializeField] private Material treeMaterial;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    private void Awake()
    {
        GenerateTree();
    }

    [ContextMenu("Generate Tree Mesh")]
    public void GenerateTree()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();

        if (treeMaterial != null)
        {
            meshRenderer.sharedMaterial = treeMaterial;
        }

        Mesh treeMesh = new Mesh();
        treeMesh.name = "Procedural LowPoly Tree";

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Color> vertexColors = new List<Color>(); // Red = 0 (Bark), Red = 1 (Leaves)

        // 1. BUILD LOW POLY TRUNK (Cylinder Cone)
        for (int i = 0; i <= trunkSegments; i++)
        {
            float angle = (i / (float)trunkSegments) * Mathf.PI * 2f;
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);

            // Bottom Ring
            vertices.Add(new Vector3(cos * trunkRadiusBottom, 0, sin * trunkRadiusBottom));
            vertexColors.Add(new Color(0, 0, 0, 1)); // Trunk mask

            // Top Ring
            vertices.Add(new Vector3(cos * trunkRadiusTop, trunkHeight, sin * trunkRadiusTop));
            vertexColors.Add(new Color(0, 0, 0, 1)); // Trunk mask
        }

        for (int i = 0; i < trunkSegments; i++)
        {
            int b1 = i * 2;
            int t1 = b1 + 1;
            int b2 = (i + 1) * 2;
            int t2 = b2 + 1;

            triangles.Add(b1); triangles.Add(t1); triangles.Add(b2);
            triangles.Add(b2); triangles.Add(t1); triangles.Add(t2);
        }

        // 2. BUILD FOLIAGE CONE LAYERS
        float currentHeight = foliageStartHeight;
        float currentRadius = baseFoliageRadius;

        for (int layer = 0; layer < foliageLayers; layer++)
        {
            int baseVertIndex = vertices.Count;

            // Cone Base Center Point
            Vector3 tipPos = new Vector3(0, currentHeight + layerHeight, 0);

            for (int i = 0; i <= foliageSegments; i++)
            {
                float angle = (i / (float)foliageSegments) * Mathf.PI * 2f;

                // Add slight low-poly vertex noise for organic look
                float randomOffset = Mathf.Sin(i * 3f + layer) * 0.1f;
                float radius = Mathf.Max(0.1f, currentRadius + randomOffset);

                float cos = Mathf.Cos(angle);
                float sin = Mathf.Sin(angle);

                // Base Ring Vertices
                vertices.Add(new Vector3(cos * radius, currentHeight, sin * radius));
                vertexColors.Add(new Color(1, 0, 0, 1)); // Leaves mask

                // Tip Vertex
                vertices.Add(tipPos);
                vertexColors.Add(new Color(1, 0, 0, 1)); // Leaves mask
            }

            for (int i = 0; i < foliageSegments; i++)
            {
                int b1 = baseVertIndex + (i * 2);
                int t1 = b1 + 1;
                int b2 = baseVertIndex + ((i + 1) * 2);

                triangles.Add(b1);
                triangles.Add(t1);
                triangles.Add(b2);
            }

            // Move up to next layer
            currentHeight += layerHeight * 0.65f;
            currentRadius = Mathf.Max(0.3f, currentRadius - radiusShrinkPerLayer);
        }

        // Apply generated geometry to mesh
        treeMesh.SetVertices(vertices);
        treeMesh.SetTriangles(triangles, 0);
        treeMesh.SetColors(vertexColors);

        // Calculate Flat Low-Poly Normals
        treeMesh.RecalculateBounds();
        treeMesh.RecalculateNormals();

        meshFilter.mesh = treeMesh;
    }
}