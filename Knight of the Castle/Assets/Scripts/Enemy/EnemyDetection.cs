using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyDetection : MonoBehaviour
{
    public enum TargetType
    {
        None,
        KingPlayer,
        DefenseTower,
        Castle
    }

    [Header("Detection Layers")]
    [SerializeField] private LayerMask playerMask;
    [SerializeField] private LayerMask towerMask;
    [SerializeField] private LayerMask obstacleLayerMask; // For Line of Sight blocking

    [Header("Behavior Tuning")]
    [SerializeField] private float targetCommitTime = 2.0f;
    [SerializeField] private float castlePriorityDistance = 5.0f;

    private EnemyController enemyController;
    private NavMeshAgent agent;
    
    private Transform ultimateTarget; // The Castle
    private Transform currentTarget;
    private TargetType currentTargetType = TargetType.None;
    private float targetLockTimer;

    public Transform CurrentTarget => currentTarget;
    public Transform UltimateTarget => ultimateTarget;
    public TargetType CurrentTargetType => currentTargetType;

    private WaitForSeconds delayWait = new WaitForSeconds(0.2f);
    private Collider[] detectionBuffer = new Collider[10];

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        enemyController = GetComponent<EnemyController>();
    }

    public void SetupDetection(Transform castleTransform)
    {
        ultimateTarget = castleTransform;
        currentTarget = null;
        currentTargetType = TargetType.None;
        targetLockTimer = 0f;

        StopAllCoroutines();
        StartCoroutine(EvaluateTargetRoutine());
    }

    private void Update()
    {
        if (enemyController == null || enemyController.IsDead || !agent.enabled) return;

        // Manage commitment lock duration
        if (targetLockTimer > 0f)
        {
            targetLockTimer -= Time.deltaTime;
        }

        // Drop target safely if it gets destroyed or goes out of physical evaluation
        if (currentTarget != null && !TargetIsValid(currentTarget, currentTargetType))
        {
            ForceClearTarget();
        }

        // Command NavMeshAgent execution
        Transform destination = currentTarget != null ? currentTarget : ultimateTarget;
        if (destination != null)
        {
            agent.SetDestination(destination.position);
        }
    }

    private IEnumerator EvaluateTargetRoutine()
    {
        while (enemyController != null && !enemyController.IsDead)
        {
            yield return delayWait;
            EvaluateTarget();
        }
    }

    private void EvaluateTarget()
    {
        if (ultimateTarget == null) return;

        float distanceToCastle = Vector3.Distance(transform.position, ultimateTarget.position);

        // Rule 6: Castle Proximity Has Higher Priority
        if (distanceToCastle <= castlePriorityDistance)
        {
            CommitTarget(ultimateTarget, TargetType.Castle);
            return;
        }

        // If target lock is active and target remains clean/valid, stick with it
        if (targetLockTimer > 0f && currentTarget != null && TargetIsValid(currentTarget, currentTargetType))
        {
            return;
        }

        // Collect prospective environmental targets
        Transform visiblePlayer = FindTargetInLayer(playerMask);
        Transform visibleTower = FindTargetInLayer(towerMask);

        // Priority Hierarchy Evaluation
        // Priority 1: Immediate Valid Player
        if (visiblePlayer != null)
        {
            CommitTarget(visiblePlayer, TargetType.KingPlayer);
            return;
        }

        // Priority 2 & 3: Committed Tower or newly detected Tower
        if (visibleTower != null)
        {
            CommitTarget(visibleTower, TargetType.DefenseTower);
            return;
        }

        // Priority 5: Fallback default objective (Castle)
        CommitTarget(ultimateTarget, TargetType.Castle);
    }

    private void CommitTarget(Transform newTarget, TargetType type)
    {
        if (currentTarget == newTarget && currentTargetType == type) return;

        currentTarget = newTarget;
        currentTargetType = type;
        
        // Lock applies to transient tactical threats (Player, Towers) to prevent frame flickering
        if (type == TargetType.KingPlayer || type == TargetType.DefenseTower)
        {
            targetLockTimer = targetCommitTime;
        }
        else
        {
            targetLockTimer = 0f;
        }
    }

    private void ForceClearTarget()
    {
        currentTarget = null;
        currentTargetType = TargetType.None;
        targetLockTimer = 0f;
    }

    private Transform FindTargetInLayer(LayerMask mask)
    {
        float range = enemyController.Data.detectionRange;
        int count = Physics.OverlapSphereNonAlloc(transform.position, range, detectionBuffer, mask);

        Transform closestTarget = null;
        float minDistance = Mathf.Infinity;

        for (int i = 0; i < count; i++)
        {
            Transform targetTrans = detectionBuffer[i].transform;
            IDamageable damageable = targetTrans.GetComponent<IDamageable>();

            if (damageable != null && !damageable.IsDead)
            {
                if (HasLineOfSight(targetTrans, range))
                {
                    float dist = Vector3.Distance(transform.position, targetTrans.position);
                    if (dist < minDistance)
                    {
                        minDistance = dist;
                        closestTarget = targetTrans;
                    }
                }
            }
        }
        return closestTarget;
    }

    private bool HasLineOfSight(Transform target, float checkRange)
    {
        Vector3 startPos = transform.position + Vector3.up * 0.5f;
        Vector3 targetPos = target.position + Vector3.up * 0.5f;
        Vector3 direction = targetPos - startPos;
        float distance = direction.magnitude;

        if (distance > checkRange) return false;

        // Returns true if there are no intervening layout colliders marked under obstacleLayerMask
        if (Physics.Raycast(startPos, direction.normalized, distance, obstacleLayerMask))
        {
            return false;
        }
        return true;
    }

    private bool TargetIsValid(Transform target, TargetType type)
    {
        if (target == null) return false;

        IDamageable damageable = target.GetComponent<IDamageable>();
        if (damageable == null || damageable.IsDead) return false;

        // Castle is universally clean/valid until destroyed
        if (type == TargetType.Castle) return true;

        // Players and Defense Towers must remain within operational detection bounds
        float actualDistance = Vector3.Distance(transform.position, target.position);
        if (actualDistance > enemyController.Data.detectionRange) return false;

        return true;
    }
}