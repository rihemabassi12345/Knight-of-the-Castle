using UnityEngine;

public class Cam : MonoBehaviour
{
    [Header("Targeting")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0f, 1.5f, -10f);

    [Header("Smoothing")]
    [SerializeField] private float smoothTime = 0.3f;
    [SerializeField] private float lookAheadDistance = 2.0f;
    [SerializeField] private float lookAheadSpeed = 3.0f;

    [Header("Level Bounds (Optional)")]
    [SerializeField] private bool useBounds = false;
    [SerializeField] private float minX = -50f;
    [SerializeField] private float maxX = 50f;
    [SerializeField] private float minY = -5f;
    [SerializeField] private float maxY = 10f;

    private Vector3 currentVelocity = Vector3.zero;
    private float currentLookAheadX = 0f;
    private float lastTargetX;

    private void Start()
    {
        if (target != null)
        {
            lastTargetX = target.position.x;
            transform.position = target.position + offset;
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        float moveDeltaX = target.position.x - lastTargetX;
        lastTargetX = target.position.x;

        float targetLookAheadX = 0f;
        if (Mathf.Abs(moveDeltaX) > 0.001f)
        {
            targetLookAheadX = Mathf.Sign(moveDeltaX) * lookAheadDistance;
        }

        currentLookAheadX = Mathf.Lerp(
            currentLookAheadX,
            targetLookAheadX,
            Time.deltaTime * lookAheadSpeed
        );

        Vector3 targetPosition = target.position + offset + new Vector3(currentLookAheadX, 0f, 0f);


        Vector3 smoothedPosition = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref currentVelocity,
            smoothTime
        );

        if (useBounds)
        {
            smoothedPosition.x = Mathf.Clamp(smoothedPosition.x, minX, maxX);
            smoothedPosition.y = Mathf.Clamp(smoothedPosition.y, minY, maxY);
        }

        transform.position = smoothedPosition;
    }
}
