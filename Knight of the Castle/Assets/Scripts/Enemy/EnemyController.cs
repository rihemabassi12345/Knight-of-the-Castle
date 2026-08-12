using UnityEngine;
using UnityEngine.AI;
using System;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemyDetection))]
[RequireComponent(typeof(EnemyCombat))]
public class EnemyController : MonoBehaviour, IDamageable
{
    [SerializeField] private EnemyData baseData; 
    
    public float CurrentHealth { get; private set; }
    public EnemyData Data => baseData;
    public bool IsDead { get; private set; }

    private NavMeshAgent agent;
    private Collider enemyCollider;
    private EnemyDetection enemyDetection;
    private EnemyCombat enemyCombat;
    
    // Event خفيف لإعلام الـ Spawner بالموت
    public static event Action<EnemyController> OnEnemyDeath;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        enemyCollider = GetComponent<Collider>();
        enemyDetection = GetComponent<EnemyDetection>();
        enemyCombat = GetComponent<EnemyCombat>();
    }

    public void InitializeEnemy(Vector3 spawnPosition, Transform castleTarget)
    {
        transform.position = spawnPosition;
        CurrentHealth = baseData.health; 
        IsDead = false;

        if (enemyCollider != null) enemyCollider.enabled = true;
        
        if (agent != null)
        {
            agent.speed = baseData.speed;
            agent.stoppingDistance = baseData.attackRange;
            agent.enabled = true;
        }

        gameObject.SetActive(true);

        // تفعيل الأجزاء الأخرى الموديلار
        enemyDetection.SetupDetection(castleTarget);
        enemyCombat.ResetCombat();
    }

    public void TakeDamage(float damage)
    {
        if (IsDead) return;

        CurrentHealth -= damage;

        if (CurrentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        IsDead = true;
        
        if (agent != null) agent.enabled = false;
        if (enemyCollider != null) enemyCollider.enabled = false;

        OnEnemyDeath?.Invoke(this);
        gameObject.SetActive(false);
    }
}