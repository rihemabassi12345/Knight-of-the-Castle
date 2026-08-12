using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyDetection : MonoBehaviour
{
    [Header("Detection Layers")]
    [SerializeField] private LayerMask targetMask;      
    [SerializeField] private LayerMask obstacleMask;    
    
    [Header("FOV Angle")]
    [Range(0, 360)] public float viewAngle = 120f;      

    private EnemyController enemyController;
    private NavMeshAgent agent;
    private Transform ultimateTarget;                   
    private Transform currentTarget;                    

    public Transform CurrentTarget => currentTarget;     

    private WaitForSeconds delayWait = new WaitForSeconds(0.2f);
    private Collider[] targetsInRadius = new Collider[10];

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        enemyController = GetComponent<EnemyController>();
    }

    public void SetupDetection(Transform castleTransform)
    {
        ultimateTarget = castleTransform;
        currentTarget = null;
        
        StopAllCoroutines();
        StartCoroutine(FindTargetsRoutine());
    }

    private void Update()
    {
        if (enemyController == null || enemyController.IsDead || !agent.enabled) return;

        // التحقق من صحة الهدف الحالي
        if (currentTarget != null && !TargetIsValid(currentTarget))
        {
            currentTarget = null;
        }

        // تحديد الوجهة
        if (currentTarget == null)
        {
            if (ultimateTarget != null)
            {
                agent.SetDestination(ultimateTarget.position);
            }
        }
        else
        {
            agent.SetDestination(currentTarget.position);
        }
    }

    private IEnumerator FindTargetsRoutine()
    {
        while (enemyController != null && !enemyController.IsDead)
        {
            yield return delayWait;
            FindVisibleTargets();
        }
    }

 private void FindVisibleTargets()
{
    float radius = enemyController.Data.detectionRange;
    int count = Physics.OverlapSphereNonAlloc(transform.position, radius, targetsInRadius, targetMask);

    Transform closestTarget = null;
    float closestDistance = Mathf.Infinity;

    for (int i = 0; i < count; i++)
    {
        Transform target = targetsInRadius[i].transform;
        
        // تجنب استهداف القلعة الرئيسية كهدف فرعي
        if (target == ultimateTarget) continue;

        IDamageable damageable = target.GetComponent<IDamageable>();
        if (damageable == null || damageable.IsDead) continue;

        Vector3 offset = target.position - transform.position;
        float dstToTarget = offset.magnitude;
        Vector3 dirToTarget = offset / dstToTarget;

        // 1. إذا كان الهدف قريباً جداً (مثل جدار أو برج يقف في طريقه)، استهدفه بدون شرط زاوية الرؤية
        bool isCloseObstacle = dstToTarget <= enemyController.Data.attackRange + 1f;

        // 2. إذا كان بعيداً قليلاً، تحقق من زاوية الرؤية FOV
        bool isInFieldOfView = Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2f;

        if (isCloseObstacle || isInFieldOfView)
        {
            // التأكد من عدم وجود عائق صلب آخر يحجب الرؤية تماماً
            if (!Physics.Raycast(transform.position + Vector3.up * 0.5f, dirToTarget, dstToTarget, obstacleMask))
            {
                if (dstToTarget < closestDistance)
                {
                    closestDistance = dstToTarget;
                    closestTarget = target;
                }
            }
        }
    }

    // تحديث الهدف الحالي
    currentTarget = closestTarget;
}
    private bool TargetIsValid(Transform target)
    {
        if (target == null) return false;
        
        IDamageable damageable = target.GetComponent<IDamageable>();
        if (damageable == null || damageable.IsDead) return false;
        
        if (Vector3.Distance(transform.position, target.position) > enemyController.Data.detectionRange) 
            return false;

        return true;
    }
}