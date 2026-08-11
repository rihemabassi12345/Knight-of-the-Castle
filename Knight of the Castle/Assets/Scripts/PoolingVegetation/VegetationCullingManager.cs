using System.Collections.Generic;
using UnityEngine;

public class VegetationCullingManager : MonoBehaviour
{
    [System.Serializable]
    public struct VegetationType
    {
        public string name;
        public GameObject prefab;
        public Vector3 extents; // Half-size bounds offset
        public int maxPoolSize; // Absolute max allowed instances on screen
    }

    public struct VegetationData
    {
        public int typeIndex;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
        public Bounds bounds;
        public GameObject activeInstance;
    }

    [Header("Camera & Culling Settings")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float maxDrawDistance = 80f;
    [SerializeField] private float checkInterval = 0.05f;

    [Header("No-Spawn Zone Settings")]
    [Tooltip("Layers representing zones where vegetation cannot spawn (e.g., Buildings, NoSpawnZones)")]
    [SerializeField] private LayerMask exclusionLayerMask;

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

        _frustumPlanes = GeometryUtility.CalculateFrustumPlanes(mainCamera);
        EvaluateVegetationCulling(currentCamPos);
    }

    private void EvaluateVegetationCulling(Vector3 camPos)
    {
        float sqrMaxDistance = maxDrawDistance * maxDrawDistance;

        for (int i = 0; i < _allVegetationData.Count; i++)
        {
            VegetationData data = _allVegetationData[i];

            float sqrDist = (data.position - camPos).sqrMagnitude;
            bool isWithinDistance = sqrDist <= sqrMaxDistance;
            bool isVisible = isWithinDistance && GeometryUtility.TestPlanesAABB(_frustumPlanes, data.bounds);

            if (isVisible)
            {
                if (data.activeInstance == null)
                {
                    // If pool limit is reached, Get() returns null and spawning is prevented
                    data.activeInstance = _pools[data.typeIndex].Get(data.position, data.rotation, data.scale);
                }
            }
            else
            {
                if (data.activeInstance != null)
                {
                    _pools[data.typeIndex].Release(data.activeInstance);
                    data.activeInstance = null;
                }
            }

            _allVegetationData[i] = data;
        }
    }

    /// <summary>
    /// Checks whether a position collides with exclusion zones. Returns true if safe to spawn.
    /// </summary>
    public bool IsPositionValid(Vector3 position, Vector3 extents, Quaternion rotation)
    {
        if (exclusionLayerMask == 0) return true; // No mask configured

        // Check if point collides with any exclusion colliders
        Collider[] hitColliders = Physics.OverlapBox(position + new Vector3(0, extents.y, 0), extents, rotation, exclusionLayerMask);
        return hitColliders.Length == 0;
    }

    public bool RegisterVegetationItem(int typeIndex, Vector3 pos, Quaternion rot, Vector3 scale)
    {
        Vector3 extents = Vector3.Scale(vegetationTypes[typeIndex].extents, scale);

        // Check if position overlaps with an exclusion zone BoxCollider/Layer
        if (!IsPositionValid(pos, extents, rot))
        {
            return false; // Registration blocked by exclusion zone
        }

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

        return true;
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
                vegetationTypes[i].maxPoolSize,
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