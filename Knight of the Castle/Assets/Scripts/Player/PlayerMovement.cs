using UnityEngine;

public enum LerpType
{
    MoveToward,
    SmoothDamp,
    Lerp
}

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Enum")]
    [SerializeField] private LerpType lerpType = LerpType.SmoothDamp;

    [Header("Properties")]
    [SerializeField] private float playerSpeed = 8f;
    [SerializeField] private float playerAccSpeed = 50f;
    [SerializeField] private float playerDeccSpeed = 40f;
    [SerializeField] private float rotationSpeed = 12f;

    [Header("References")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Animator anim;
    [SerializeField] private SwordPlayer swordPlayer;

    private Vector2 inputDir;
    private Vector2 targetVelocity;
    private Vector2 currentVelocity;
    private Vector2 newVelocity;
    private Vector2 currentVelocitySmoothing;

    [SerializeField] private float moveSmoothTime = 0.08f;

    private void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (anim == null) anim = GetComponentInChildren<Animator>();
        if (swordPlayer == null) swordPlayer = GetComponent<SwordPlayer>();

        rb.constraints = RigidbodyConstraints.FreezeRotationX |
                         RigidbodyConstraints.FreezeRotationY |
                         RigidbodyConstraints.FreezeRotationZ;
    }

    private void Update()
    {
        Inputs();
        Animate();
        RotateTowardsMovement();
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void Inputs()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        inputDir = new Vector2(moveX, moveY).normalized;
    }

    private void Move()
    {
        float speedMultiplier = 1.0f;

        // Apply movement modifier during combat instead of complete freeze
        if (swordPlayer != null && swordPlayer.IsAttacking)
        {
            speedMultiplier = swordPlayer.movementMultiplierDuringAttack;
        }

        float effectiveSpeed = playerSpeed * speedMultiplier;
        targetVelocity = inputDir * effectiveSpeed;

        float accelRate = (inputDir.sqrMagnitude > 0.01f) ? playerAccSpeed : playerDeccSpeed;
        currentVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.z);

        ApplyLerpType(lerpType, accelRate);

        rb.linearVelocity = new Vector3(newVelocity.x, rb.linearVelocity.y, newVelocity.y);
    }

    private void ApplyLerpType(LerpType type, float accelRate)
    {
        switch (type)
        {
            case LerpType.Lerp:
                newVelocity = Vector2.Lerp(currentVelocity, targetVelocity, accelRate * Time.fixedDeltaTime);
                break;
            case LerpType.MoveToward:
                newVelocity = Vector2.MoveTowards(currentVelocity, targetVelocity, accelRate * Time.fixedDeltaTime);
                break;
            case LerpType.SmoothDamp:
                newVelocity = Vector2.SmoothDamp(currentVelocity, targetVelocity, ref currentVelocitySmoothing, moveSmoothTime, Mathf.Infinity, Time.fixedDeltaTime);
                break;
        }
    }

    private void Animate()
    {
        if (anim == null) return;

        // Drive leg locomotion based on actual horizontal movement
        float currentSpeed = new Vector2(rb.linearVelocity.x, rb.linearVelocity.z).magnitude;
        anim.SetFloat("Speed", Mathf.Lerp(anim.GetFloat("Speed"), currentSpeed, Time.deltaTime * 10f));
    }

    private void RotateTowardsMovement()
    {
        if (inputDir.sqrMagnitude > 0.01f)
        {
            float targetAngle = Mathf.Atan2(inputDir.x, inputDir.y) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(0f, targetAngle, 0f);

            // Slightly reduce rotation speed during swings for heavy feeling
            float activeRotSpeed = (swordPlayer != null && swordPlayer.IsAttacking)
                ? rotationSpeed * 0.65f
                : rotationSpeed;

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, activeRotSpeed * Time.deltaTime);
        }
    }
}