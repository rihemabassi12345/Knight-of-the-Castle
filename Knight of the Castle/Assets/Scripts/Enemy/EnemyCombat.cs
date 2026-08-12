using UnityEngine;

public class EnemyCombat : MonoBehaviour
{
    [Header("Combat Settings")]
    [SerializeField] private float attackCooldown = 1.5f; 

    private EnemyController enemyController;
    private EnemyDetection enemyDetection;
    private float attackTimer;

    private void Awake()
    {
        enemyController = GetComponent<EnemyController>();
        enemyDetection = GetComponent<EnemyDetection>();
    }

    public void ResetCombat()
    {
        attackTimer = 0f;
    }

    private void Update()
    {
        if (enemyController.IsDead) return;

        Transform target = enemyDetection.CurrentTarget;
        if (target == null) return;

        // Optimization: مقارنة المربعات المربعة (SqredDistance) أسرع بـ 10 مرات من Vector3.Distance
        Vector3 offset = target.position - transform.position;
        float sqrLen = offset.sqrMagnitude;
        float sqrAttackRange = enemyController.Data.attackRange * enemyController.Data.attackRange;

        if (sqrLen <= sqrAttackRange)
        {
            attackTimer += Time.deltaTime;

            if (attackTimer >= attackCooldown)
            {
                attackTimer = 0f;
                AttackTarget(target);
            }
        }
    }

    private void AttackTarget(Transform target)
    {
        IDamageable damageableTarget = target.GetComponent<IDamageable>();
        
        if (damageableTarget != null && !damageableTarget.IsDead)
        {
            damageableTarget.TakeDamage(enemyController.Data.damage);
        }
    }
}