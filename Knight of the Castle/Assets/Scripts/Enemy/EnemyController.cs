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
    public Animator animator; // الـ Animator الخاص بالـ animation
    
    // Event لإعلام الـ Spawner بالموت[cite: 3]
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

        // تفعيل الأجزاء الموديلار الأخرى[cite: 3]
        enemyDetection.SetupDetection(castleTarget);
        enemyCombat.ResetCombat();

        // إعادة تشغيل الـ Animator إذا كان واقفا
        if (animator != null)
        {
            animator.SetBool("IsDead", false);
            animator.SetFloat("Speed", 0f);
        }
    }

    private void Update()
    {
        if (IsDead || agent == null || animator == null || !agent.enabled) return;

        // حساب السرعة الحالية للـ NavMeshAgent
        // باش الإنيميشن تمشي وتجري متناسقة مع الحركة في الـ خريطة
        float currentSpeed = agent.velocity.magnitude;
        
        // تمرير السرعة للـ Animator (تأكد إن إسم المتغير في الـ Animator هو "Speed")
        animator.SetFloat("Speed", currentSpeed);
    }

    public void TakeDamage(float damage)
    {
        if (IsDead) return;

        CurrentHealth -= damage;

        // تشغيل إنيميشن تلقي الضرر (Hurt / TakeDamage) إذا تحب تزيدها
        if (animator != null)
        {
            animator.SetTrigger("Hit");
        }

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

        // تشغيل إنيميشن الموت
        if (animator != null)
        {
            animator.SetBool("IsDead", true);
            animator.SetTrigger("Die");
        }

        OnEnemyDeath?.Invoke(this);
        
        // إذا كان عندك إنيميشن موت طويلة، تنجم تعمل لقطة إخفاء العدو بعد ثانية أو اثنين 
        // أو تخليه توا هكا كيف ما كان في الكود القديم متاعك:
        gameObject.SetActive(false);
    }
}