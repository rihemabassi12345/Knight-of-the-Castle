using System.Collections.Generic;
using UnityEngine;

public class VegetationCullingManager : MonoBehaviour
{
    [System.Serializable]
    public struct VegetationType
    {
        public string name;
        public GameObject prefab;
        public Vector3 extents; // Half-size bounds offset (e.g., Tree = x:0.5, y:2.0, z:0.5)
        public int initialPoolSize;
    }

    public struct VegetationData
    {
        public int typeIndex;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
        public Bounds bounds;          // Fast AABB struct
        public GameObject activeInstance; // Reference to pooled GameObject (null when culled)
    }

    [Header("Camera & Culling Settings")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float maxDrawDistance = 80f;
    [SerializeField] private float checkInterval = 0.05f; // Runs 20 times/sec instead of every frame

    [Header("Vegetation Definitions")]
    [SerializeField] private VegetationType[] vegetationTypes;

    // Internal State
    private readonly List<VegetationData> _allVegetationData = new List<VegetationData>();
    private VegetationPool[] _pools;
    private Plane[] _frustumPlanes;

    private float _timer;
    private Vector3 _lastCamPosition;
    private Quaternion _lastCamRotation;

    private void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;

        InitializePools();
        GenerateDemoWorld(2000);
    }

    private void Update()
    {
        _timer += Time.deltaTime;
        if (_timer < checkInterval) return;
        _timer = 0f;

        Transform camTransform = mainCamera.transform;
        Vector3 currentCamPos = camTransform.position;
        Quaternion currentCamRot = camTransform.rotation;

        if (currentCamPos == _lastCamPosition && currentCamRot == _lastCamRotation)
        {
            return;
        }

        _lastCamPosition = currentCamPos;
        _lastCamRotation = currentCamRot;

        // Recalculate camera frustum planes
        _frustumPlanes = GeometryUtility.CalculateFrustumPlanes(mainCamera);

        // Run optimized check loop
        EvaluateVegetationCulling(currentCamPos);
    }

    private void EvaluateVegetationCulling(Vector3 camPos)
    {
        float sqrMaxDistance = maxDrawDistance * maxDrawDistance;

        for (int i = 0; i < _allVegetationData.Count; i++)
        {
            VegetationData data = _allVegetationData[i];

            // OPTIMIZATION 2: Distance check using sqrMagnitude (faster than Vector3.Distance)
            float sqrDist = (data.position - camPos).sqrMagnitude;
            bool isWithinDistance = sqrDist <= sqrMaxDistance;

            // OPTIMIZATION 3: AABB Frustum Check using lightweight struct Bounds
            bool isVisible = isWithinDistance && GeometryUtility.TestPlanesAABB(_frustumPlanes, data.bounds);

            if (isVisible)
            {
                // Visible & Not Spawned -> Get from pool
                if (data.activeInstance == null)
                {
                    data.activeInstance = _pools[data.typeIndex].Get(data.position, data.rotation, data.scale);
                }
            }
            else
            {
                // Invisible & Currently Active -> Recycle back to pool
                if (data.activeInstance != null)
                {
                    _pools[data.typeIndex].Release(data.activeInstance);
                    data.activeInstance = null;
                }
            }

            _allVegetationData[i] = data; // Write struct back to list
        }
    }

    /// <summary>
    /// Call this function from your custom terrain generator / world builders.
    /// </summary>
    public void RegisterVegetationItem(int typeIndex, Vector3 pos, Quaternion rot, Vector3 scale)
    {
        Vector3 extents = Vector3.Scale(vegetationTypes[typeIndex].extents, scale);
        Bounds aabbBounds = new Bounds(pos + new Vector3(0, extents.y, 0), extents * 2f);

        _allVegetationData.Add(new VegetationData
        {
            typeIndex = typeIndex,
            position = pos,
            rotation = rot,
            scale = scale,
            bounds = aabbBounds,
            activeInstance = null
        });
    }

    private void InitializePools()
    {
        _pools = new VegetationPool[vegetationTypes.Length];

        for (int i = 0; i < vegetationTypes.Length; i++)
        {
            GameObject container = new GameObject($"Pool_{vegetationTypes[i].name}");
            container.transform.SetParent(transform);

            _pools[i] = new VegetationPool(
                vegetationTypes[i].prefab,
                vegetationTypes[i].initialPoolSize,
                container.transform
            );
        }
    }

    private void GenerateDemoWorld(int count)
    {
        for (int i = 0; i < count; i++)
        {
            int randomType = Random.Range(0, vegetationTypes.Length);
            Vector3 randomPos = new Vector3(Random.Range(-100f, 100f), 0, Random.Range(-100f, 100f));
            Quaternion randomRot = Quaternion.Euler(0, Random.Range(0, 360), 0);
            Vector3 randomScale = Vector3.one * Random.Range(0.8f, 1.3f);

            RegisterVegetationItem(randomType, randomPos, randomRot, randomScale);
        }
    }
}