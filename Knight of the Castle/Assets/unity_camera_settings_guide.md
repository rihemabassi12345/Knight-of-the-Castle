# Unity Top-Down & Isometric Camera Configuration Guide
## Complete Technical Setup, Inspector Parameters, and Production-Ready Code

---

### Overview

This guide provides a complete, production-ready specification for setting up a stable, flexible top-down/isometric camera system in **Unity**. It covers standard `Camera` component values, transform hierarchy setups, custom script parameters, and **Unity Cinemachine** virtual camera configurations.

---

## 1. Main Camera Component Inspector Settings

Configure the primary `Camera` component on your `Main Camera` object as follows:

| Property | Recommended Value | Context & Purpose |
| :--- | :--- | :--- |
| **Projection** | `Perspective` (or `Orthographic`) | **Perspective:** Gives natural depth & scale (3D assets pop).<br>**Orthographic:** Ideal for pure flat 2D/isometric strategy layouts. |
| **Field of View (FOV)** | `30°` – `45°` *(Perspective)* | Low FOV with higher distance creates an isometric look without edge distortion. |
| **Clipping Planes - Near** | `0.3` | Prevents near-object clipping while avoiding depth buffer precision issues. |
| **Clipping Planes - Far** | `300` – `500` | Keeps rendering tight to optimize performance and shadow draw distance. |
| **Clear Flags** | `Skybox` or `Solid Color` | Use `Solid Color` (e.g., dark dark slate) for stylized map edges, or `Skybox` for open worlds. |
| **Allow HDR** | `Enabled` | Required for post-processing pipelines (Bloom, Color Grading, Tonemapping). |
| **Allow MSAA** | `Enabled` | Smooths geometry edges (Anti-Aliasing) for stylized and low-poly art styles. |

---

## 2. Rig Hierarchy & Transform Configuration

Do **not** attach movement scripts directly to a lone camera. Use a **3-Tier Hierarchy Anchor System** for clean separation of rotation, pitch, and position:

```
[Camera_Rig_Anchor]         --> Controls position (X, Z) and Y-yaw rotation
  └── [Camera_Pitch_Pivot]  --> Controls tilt angle (X-axis pitch)
        └── [Main Camera]   --> Holds the Camera component & Z-offset distance
```

### Transform Default Values

* **Rig Anchor Transform:**
  * Position: `(0, 0, 0)`
  * Rotation: `(0, 45, 0)` *(45° Y-yaw gives standard isometric alignment)*
* **Pitch Pivot Transform:**
  * Position: `(0, 0, 0)`
  * Rotation: `(45, 0, 0)` *(45° X-pitch gives ideal top-down viewing angle)*
* **Main Camera Transform:**
  * Local Position: `(0, 0, -25)` *(Initial distance offset)*
  * Local Rotation: `(0, 0, 0)`

---

## 3. C# Production Script Parameter Guide

Below are the complete settings and script implementation for custom camera controls.

### 3.1 Inspector Parameter Reference

| Parameter | Recommended Range | Description |
| :--- | :--- | :--- |
| **Move Speed** | `15.0` – `25.0` | Base keyboard/pan tracking velocity. |
| **Smooth Time** | `0.08` – `0.15` | Damping time for smooth movement inertia. |
| **Min Zoom** | `8.0` – `12.0` | Closest camera distance (detailed view). |
| **Max Zoom** | `35.0` – `50.0` | Farthest camera distance (tactical/base view). |
| **Zoom Sensitivity** | `8.0` – `15.0` | Mouse scroll wheel input scaling. |
| **Zoom Smooth Speed**| `10.0` – `15.0` | Interpolation speed (`Lerp`) for zoom transitions. |

### 3.2 Complete Camera Controller Script (`IsometricCameraController.cs`)

```csharp
using UnityEngine;

public class IsometricCameraController : MonoBehaviour
{
    [Header("Target & Following")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 targetOffset = Vector3.zero;
    [SerializeField] private float smoothTime = 0.12f;

    [Header("Movement & Rotation")]
    [SerializeField] private float panSpeed = 20f;
    [SerializeField] private float yawAngle = 45f;
    [SerializeField] private float pitchAngle = 45f;

    [Header("Zoom Settings")]
    [SerializeField] private float minZoom = 10f;
    [SerializeField] private float maxZoom = 40f;
    [SerializeField] private float zoomSensitivity = 10f;
    [SerializeField] private float zoomSmoothSpeed = 12f;

    private Transform pitchPivot;
    private Transform cameraTransform;

    private Vector3 currentVelocity;
    private float targetZoom;
    private float currentZoom;

    private void Awake()
    {
        // Build or reference hierarchy setup
        pitchPivot = transform.GetChild(0);
        cameraTransform = pitchPivot.GetChild(0);

        targetZoom = (minZoom + maxZoom) * 0.5f;
        currentZoom = targetZoom;

        // Apply static angles
        transform.rotation = Quaternion.Euler(0f, yawAngle, 0f);        
        pitchPivot.localRotation = Quaternion.Euler(pitchAngle, 0f, 0f);
    }

    private void LateUpdate()
    {
        HandleZoom();
        HandlePosition();
    }

    private void HandleZoom()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        targetZoom -= scrollInput * zoomSensitivity;
        targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);

        currentZoom = Mathf.Lerp(currentZoom, targetZoom, Time.deltaTime * zoomSmoothSpeed);
        cameraTransform.localPosition = new Vector3(0f, 0f, -currentZoom);
    }

    private void HandlePosition()
    {
        Vector3 targetPosition = transform.position;

        if (target != null)
        {
            targetPosition = target.position + targetOffset;
        }
        else
        {
            // Free Camera Pan Controls
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            Vector3 moveInput = new Vector3(horizontal, 0f, vertical);
            Vector3 forward = transform.forward;
            Vector3 right = transform.right;
            forward.y = 0f;
            right.y = 0f;

            Vector3 moveDirection = (forward.Normalized * moveInput.z + right.Normalized * moveInput.x).normalized;
            targetPosition += moveDirection * panSpeed * Time.deltaTime;
        }

        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothTime);
    }
}
```

---

## 4. Cinemachine Virtual Camera Setup (Alternative Workflow)

If using Unity's **Cinemachine** package, create a `CinemachineVirtualCamera` with the following component configurations:

### Body Settings (`Framing Transposer`)
* **Follow Target:** Your Player / Focus Object
* **LookAt Target:** *(Leave Unassigned to prevent camera tilt distortion)*
* **Camera Distance:** `25.0`
* **Tracked Object Offset:** `(X: 0, Y: 0, Z: 0)`
* **XDamping:** `0.8`
* **YDamping:** `0.8`
* **ZDamping:** `0.8`

### Lens Settings
* **Field of View:** `35°`
* **Near Clip Plane:** `0.3`
* **Far Clip Plane:** `400`

### Orientation & Angles
Set the transform of the Virtual Camera GameObject directly to:
* **Rotation X:** `45°`
* **Rotation Y:** `45°`
* **Rotation Z:** `0°`

---

## 5. Camera Occlusion & Transparency Shader Setup

To prevent tall assets (trees, rocks, walls) from blocking the view of the player or units:

1. **Raycast Layer Mask:** Assign environmental obstacles to a specific layer (e.g., `Environment`).
2. **Raycast Method:** Cast a ray from `Main Camera` position to `Player` position in `LateUpdate()`.
3. **Dither/Alpha Fade:** When a ray hits an obstacle, update a `_DitherOpacity` float property on the obstacle's `Material` rather than disabling its collider/GameObject.
