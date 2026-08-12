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
public class PlayerMovement : MonoBehaviour, IDamageable
{
    [Header("Enum")]
    [SerializeField] private LerpType lerpType;

    [Header("Health Setup")]
    [SerializeField] private float maxHealth = 100f;
    public float CurrentHealth { get; private set; }
    public bool IsDead { get; private set; }

    [Header("Properties")]
    [SerializeField] private float PlayerSpeed = 8f;
    [SerializeField] private float PlayerAccSpeed = 50f;
    [SerializeField] private float PlayerDeccSpeed = 40f;
    [SerializeField] private float rotationSpeed = 15f; 

    [Header("Inputs & State")]
    [SerializeField] private float moveX;
    [SerializeField] private float moveY;

    [Header("References")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Animator anim;

    [Header("Debug Velocity Info")]
    [SerializeField] private Vector2 inputDir;
    [SerializeField] private float accelRate;
    [SerializeField] private Vector2 targetVelocity;
    [SerializeField] private Vector2 currentVelocity;
    [SerializeField] private Vector2 newVelocity;

    private Vector2 currentVelocitySmoothing;
    [SerializeField] private float moveSmoothTime = 0.1f;

    private void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (anim == null) anim = GetComponentInChildren<Animator>();

        rb.constraints = RigidbodyConstraints.FreezeRotationX |
                         RigidbodyConstraints.FreezeRotationY |
                         RigidbodyConstraints.FreezeRotationZ;

        CurrentHealth = maxHealth;
        IsDead = false;
    }

    private void Update()
    {
        if (IsDead) return;

        Inputs();
        Animate();
    }

    private void FixedUpdate()
    {
        if (IsDead) return;

        Move();
        RotateTowardsMovement();
    }

    private void Inputs()
    {
        moveX = Input.GetAxisRaw("Horizontal");
        moveY = Input.GetAxisRaw("Vertical");
    }

    private void Move()
    {
        inputDir = new Vector2(moveX, moveY).normalized;
        targetVelocity = inputDir * PlayerSpeed;

        accelRate = (inputDir.sqrMagnitude > 0.01f) ? PlayerAccSpeed : PlayerDeccSpeed;

        currentVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.z);

        ChangelerpType(lerpType);

        rb.linearVelocity = new Vector3(newVelocity.x, rb.linearVelocity.y, newVelocity.y);
    }

    private void ChangelerpType(LerpType type)
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

        float currentSpeed = new Vector2(rb.linearVelocity.x, rb.linearVelocity.z).magnitude;
        anim.SetFloat("Speed", currentSpeed);
    }

    private void RotateTowardsMovement()
    {
        if (inputDir.sqrMagnitude > 0.01f)
        {
            float targetAngle = Mathf.Atan2(inputDir.x, inputDir.y) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(0f, targetAngle, 0f);

            // استخدام MoveRotation أفضل مع Rigidbody بدلاً من transform.rotation مباشر
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime));
        }
    }

    // تطبيق واجهة IDamageable
    public void TakeDamage(float damage)
    {
        if (IsDead) return;

        CurrentHealth -= damage;
        Debug.Log($"Player hit! Current Health: {CurrentHealth}");

        if (CurrentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        IsDead = true;
        Debug.Log("Player Died!");
        // أضف أنيميشن الموت أو كود إعادة التشغيل هنا
    }
}