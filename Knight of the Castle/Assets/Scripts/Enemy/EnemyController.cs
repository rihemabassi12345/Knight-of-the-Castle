using System;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemyDetection))]
[RequireComponent(typeof(EnemyCombat))]
public class EnemyController : MonoBehaviour, IDamageable
{
    [Header("Data Configuration")]
    [SerializeField] private EnemyData baseData;

    [Header("Visual Components")]
    [SerializeField] private Animator animator;

    [Header("Debug / Health Monitor")]
    [SerializeField] private float currentHealth; 
    public float CurrentHealth => currentHealth;
    public float MaxHealth => baseData != null ? baseData.health : 0f;
    
    [SerializeField] private bool isDead;
    public bool IsDead => isDead;

    public EnemyData Data => baseData;

    private NavMeshAgent agent;
    private Collider enemyCollider;
    private EnemyDetection enemyDetection;
    private EnemyCombat enemyCombat;

    public static event Action<EnemyController> OnEnemyDeath;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        enemyCollider = GetComponent<Collider>();
        enemyDetection = GetComponent<EnemyDetection>();
        enemyCombat = GetComponent<EnemyCombat>();

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        ValidateComponents();
    }

    private void ValidateComponents()
    {
        if (baseData == null)
        {
            Debug.LogError($"[EnemyController] EnemyData is missing on {gameObject.name}!", this);
        }
    }

    public void InitializeEnemy(Vector3 spawnPosition, Transform castleTarget)
    {
        if (baseData == null)
        {
            Debug.LogError($"[EnemyController] Cannot initialize {gameObject.name} without EnemyData!", this);
            return;
        }

        transform.position = spawnPosition;
        gameObject.SetActive(true);

        // Reset Health & Status
        currentHealth = baseData.health;
        isDead = false;

        // Reset Collider
        if (enemyCollider != null)
        {
            enemyCollider.enabled = true;
        }

        // Reset NavMeshAgent
        if (agent != null)
        {
            agent.enabled = true;
            agent.speed = baseData.speed;
            agent.stoppingDistance = baseData.stoppingDistance;
            agent.isStopped = false;
            agent.ResetPath();
        }

        // Reset Animator
        if (animator != null)
        {
            animator.SetBool("IsDead", false);
            animator.SetFloat("Speed", 0f);
        }

        // Reset Sub-systems
        if (enemyDetection != null)
        {
            enemyDetection.SetupDetection(castleTarget);
        }

        if (enemyCombat != null)
        {
            enemyCombat.ResetCombat();
        }
    }

    private void Update()
    {
        if (isDead || agent == null || !agent.enabled || animator == null) return;

        float currentSpeed = agent.velocity.magnitude;
        animator.SetFloat("Speed", currentSpeed);
    }

    public void TakeDamage(float damage)
    {
        if (isDead || damage <= 0f) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0f);

        if (animator != null)
        {
            animator.SetTrigger("Hit");
        }

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return;

        isDead = true;

        if (agent != null && agent.enabled)
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.enabled = false;
        }

        if (enemyCollider != null)
        {
            enemyCollider.enabled = false;
        }

        if (animator != null)
        {
            animator.SetBool("IsDead", true);
            animator.SetTrigger("Die");
        }

        OnEnemyDeath?.Invoke(this);

        gameObject.SetActive(false);
    }
}