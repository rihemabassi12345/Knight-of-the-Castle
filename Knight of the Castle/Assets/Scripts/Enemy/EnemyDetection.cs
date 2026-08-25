using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyDetection : MonoBehaviour
{
    public enum TargetType
    {
        None,
        DefenseObject,
        Castle
    }

    [Header("Trigger Detection Bounds")]
    [SerializeField] private Vector3 triggerBoxSize = new Vector3(10f, 4f, 10f); // كبّرنا الحجم
    [SerializeField] private Vector3 triggerBoxOffset = new Vector3(0f, 1f, 0f); // المركز في وسط العدو
    [SerializeField] private LayerMask defenseLayer; 

    public EnemyController enemyController;
    public NavMeshAgent agent;

    public Transform ultimateTarget;
    public Transform currentTarget;
    public TargetType currentTargetType = TargetType.None;

    public List<Transform> detectedDefenses = new List<Transform>();

    public Transform CurrentTarget => currentTarget;
    public Transform UltimateTarget => ultimateTarget;
    public TargetType CurrentTargetType => currentTargetType;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        enemyController = GetComponent<EnemyController>();
    }

    public void SetupDetection(Transform castleTransform)
    {
        ultimateTarget = castleTransform;
        currentTarget = castleTransform;
        currentTargetType = TargetType.Castle;
        detectedDefenses.Clear();
    }

    private void Update()
    {
        if (enemyController == null || enemyController.IsDead || !agent.enabled) return;

        ScanForDefenses();
        EvaluateTargets();
        UpdateMovementDestination();
    }

    private void ScanForDefenses()
    {
        Vector3 center = transform.TransformPoint(triggerBoxOffset);
        Vector3 halfExtents = triggerBoxSize / 2f;

        // استشعار بدون التقيد بدوران العدو لضمان الدقة (Quaternion.identity)
        Collider[] hits = Physics.OverlapBox(center, halfExtents, Quaternion.identity, defenseLayer);

        foreach (Collider hit in hits)
        {
            RegisterPotentialTarget(hit);
        }
    }

    private void RegisterPotentialTarget(Collider other)
    {
        if (other == null || other.gameObject == gameObject || other.transform.root == transform.root) return;
        if (ultimateTarget != null && other.transform == ultimateTarget) return;

        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable != null && !damageable.IsDead)
        {
            if (!detectedDefenses.Contains(other.transform))
            {
                detectedDefenses.Add(other.transform);
            }
        }
    }

    private void EvaluateTargets()
    {
        // 1. تنظيف القائمة من الأهداف المدمرة أو البعيدة
        for (int i = detectedDefenses.Count - 1; i >= 0; i--)
        {
            Transform t = detectedDefenses[i];
            if (t == null || !t.gameObject.activeInHierarchy || IsTargetDead(t))
            {
                detectedDefenses.RemoveAt(i);
            }
        }

        // 2. اختيار أقرب دفاع مفعل حالياً دائماً
        Transform closestDefense = GetClosestDefense();

        if (closestDefense != null)
        {
            currentTarget = closestDefense;
            currentTargetType = TargetType.DefenseObject;
        }
        else
        {
            // 3. العودة إلى القلعة إذا ما فماش دفاعات في النطاق
            currentTarget = ultimateTarget;
            currentTargetType = TargetType.Castle;
        }
    }

    private Transform GetClosestDefense()
    {
        Transform closest = null;
        float minDistance = Mathf.Infinity;

        foreach (Transform defense in detectedDefenses)
        {
            if (defense == null) continue;

            float dist = Vector3.Distance(transform.position, defense.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                closest = defense;
            }
        }

        return closest;
    }

    private bool IsTargetDead(Transform target)
    {
        if (target == null) return true;
        IDamageable damageable = target.GetComponent<IDamageable>();
        return damageable == null || damageable.IsDead;
    }

    private void UpdateMovementDestination()
    {
        Transform destination = currentTarget != null ? currentTarget : ultimateTarget;

        if (destination != null && agent.isOnNavMesh)
        {
            agent.SetDestination(destination.position);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position + triggerBoxOffset, triggerBoxSize);
    }
}