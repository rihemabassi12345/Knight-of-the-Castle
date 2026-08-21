using System.Collections;
using UnityEngine;

public class BuildPhaseController : MonoBehaviour, IBuildPhaseListener
{
    [Header("Player & UI References")]
    [SerializeField] private GameObject playerObject;
    [SerializeField] private GameObject[] elementsToHideInBuildPhase;
    [SerializeField] private GameObject[] elementsToShowInBuildPhase;
    [SerializeField] private GameObject buildCanvas;

    [Header("Camera Configuration")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform buildCameraViewPoint;
    [SerializeField] private float transitionDuration = 1.0f;

    [Header("Placement & Grid Configuration")]
    [SerializeField] private float gridSize = 1.0f;
    [SerializeField] private LayerMask placementLayerMask;
    [SerializeField] private LayerMask deletableLayerMask;
    [SerializeField] private Material previewMaterial;
    [SerializeField] private float refundPercentage = 1.0f;

    private Vector3 originalCameraPos;
    private Quaternion originalCameraRot;
    private Transform originalCameraParent;

    private GameObject activeDefensePrefab;
    private GameObject currentPreviewObject;
    private Coroutine cameraTransitionCoroutine;
    private bool isBuildingActive = false;
    private int currentDefenseCost = 15;

    private void OnEnable()
    {
        GameEvents.OnPrepPhaseStart += OnPrepPhaseStarted;
        GameEvents.OnDayPhaseStart += OnDayPhaseStarted;
    }

    private void OnDisable()
    {
        GameEvents.OnPrepPhaseStart -= OnPrepPhaseStarted;
        GameEvents.OnDayPhaseStart -= OnDayPhaseStarted;
    }

    private void Update()
    {
        if (!isBuildingActive) return;

        UpdatePreviewPosition();

        if (Input.GetMouseButtonDown(0))
        {
            TryPlaceDefense();
        }

        if (Input.GetMouseButtonDown(1))
        {
            TryRemoveDefense();
        }
    }

    public void SelectDefenseToBuild(GameObject prefab, int cost)
    {
        activeDefensePrefab = prefab;
        currentDefenseCost = cost;

        if (currentPreviewObject != null)
        {
            Destroy(currentPreviewObject);
        }

        SpawnPreviewObject();
    }

    public void OnPrepPhaseStarted(float duration)
    {
        isBuildingActive = true;

        if (playerObject != null) playerObject.SetActive(false);

        SetUIElementsActive(elementsToHideInBuildPhase, false);
        SetUIElementsActive(elementsToShowInBuildPhase, true);

        if (buildCanvas != null) buildCanvas.SetActive(true);

        if (mainCamera != null)
        {
            Transform camTransform = mainCamera.transform;
            originalCameraPos = camTransform.position;
            originalCameraRot = camTransform.rotation;
            originalCameraParent = camTransform.parent;

            StartCameraTransition(buildCameraViewPoint.position, buildCameraViewPoint.rotation, null);
        }

        if (activeDefensePrefab != null)
        {
            SpawnPreviewObject();
        }
    }

    public void OnDayPhaseStarted(float duration)
    {
        isBuildingActive = false;

        if (currentPreviewObject != null)
        {
            Destroy(currentPreviewObject);
        }

        if (buildCanvas != null) buildCanvas.SetActive(false);

        if (mainCamera != null)
        {
            StartCameraTransition(originalCameraPos, originalCameraRot, originalCameraParent, () =>
            {
                if (playerObject != null) playerObject.SetActive(true);

                SetUIElementsActive(elementsToHideInBuildPhase, true);
                SetUIElementsActive(elementsToShowInBuildPhase, false);
            });
        }
    }

    private void SetUIElementsActive(GameObject[] elements, bool state)
    {
        if (elements == null) return;
        foreach (GameObject element in elements)
        {
            if (element != null) element.SetActive(state);
        }
    }

    private void SpawnPreviewObject()
    {
        if (activeDefensePrefab == null) return;

        currentPreviewObject = Instantiate(activeDefensePrefab);

        Collider[] colliders = currentPreviewObject.GetComponentsInChildren<Collider>();
        foreach (var col in colliders)
        {
            col.enabled = false;
        }

        Renderer[] renderers = currentPreviewObject.GetComponentsInChildren<Renderer>();
        foreach (var rend in renderers)
        {
            if (previewMaterial != null) rend.material = previewMaterial;
        }
    }

    // ==========================================
    // 3D FULL-GRID ALIGNMENT (AIR & STACKING FIX)
    // ==========================================
    private void UpdatePreviewPosition()
    {
        if (currentPreviewObject == null || mainCamera == null) return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 500f, placementLayerMask))
        {
            // Offset point outward along surface normal to resolve adjacent grid cell center
            Vector3 targetPoint = hit.point + (hit.normal * (gridSize * 0.5f));

            // Snap X, Y, and Z to 3D grid steps
            float snappedX = Mathf.Floor(targetPoint.x / gridSize) * gridSize + (gridSize * 0.5f);
            float snappedY = Mathf.Floor(targetPoint.y / gridSize) * gridSize + (gridSize * 0.5f);
            float snappedZ = Mathf.Floor(targetPoint.z / gridSize) * gridSize + (gridSize * 0.5f);

            currentPreviewObject.transform.position = new Vector3(snappedX, snappedY, snappedZ);

            // Keep object world rotation upright by default
            currentPreviewObject.transform.rotation = Quaternion.identity;
        }
    }

    private void TryPlaceDefense()
    {
        if (currentPreviewObject == null || activeDefensePrefab == null) return;

        ICoinService coinService = CoinManager.Instance;

        if (coinService != null)
        {
            if (!coinService.SpendCoins(currentDefenseCost))
            {
                Debug.LogWarning("Not enough coins to construct defense!");
                return;
            }
        }

        GameObject newDefense = Instantiate(activeDefensePrefab, currentPreviewObject.transform.position, Quaternion.identity);

        // Ensure newly placed objects inherit correct layer for deletion and raycasting
        int defenseLayer = LayerMask.NameToLayer("Defense");
        if (defenseLayer != -1)
        {
            newDefense.layer = defenseLayer;
        }
    }

    private void TryRemoveDefense()
    {
        if (mainCamera == null) return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 500f, deletableLayerMask))
        {
            GameObject targetObject = hit.collider.gameObject;

            if (hit.collider.transform.parent != null)
            {
                targetObject = hit.collider.transform.root.gameObject;
            }

            int refundAmount = Mathf.RoundToInt(currentDefenseCost * refundPercentage);

            ICoinService coinService = CoinManager.Instance;
            if (coinService != null)
            {
                coinService.AddCoins(refundAmount);
            }

            Destroy(targetObject);
        }
    }

    private void StartCameraTransition(Vector3 targetPos, Quaternion targetRot, Transform newParent, System.Action onComplete = null)
    {
        if (cameraTransitionCoroutine != null) StopCoroutine(cameraTransitionCoroutine);
        cameraTransitionCoroutine = StartCoroutine(SmoothCameraRoutine(targetPos, targetRot, newParent, onComplete));
    }

    private IEnumerator SmoothCameraRoutine(Vector3 targetPos, Quaternion targetRot, Transform newParent, System.Action onComplete)
    {
        float elapsed = 0f;
        Transform camTransform = mainCamera.transform;

        Vector3 startPos = camTransform.position;
        Quaternion startRot = camTransform.rotation;

        camTransform.SetParent(null);

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / transitionDuration);

            camTransform.position = Vector3.Lerp(startPos, targetPos, t);
            camTransform.rotation = Quaternion.Slerp(startRot, targetRot, t);

            yield return null;
        }

        camTransform.position = targetPos;
        camTransform.rotation = targetRot;
        camTransform.SetParent(newParent);

        onComplete?.Invoke();
    }
}