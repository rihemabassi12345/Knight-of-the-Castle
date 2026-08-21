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
    [SerializeField] private Vector3 triggerBoxSize = new Vector3(2.5f, 2f, 3f);
    [SerializeField] private Vector3 triggerBoxOffset = new Vector3(0f, 1f, 1.5f);
    [SerializeField] private LayerMask defenseLayer; // أضف طبقة الدفاعات من الـ Inspector

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
        currentTarget = castleTransform; // ضبط القلعة كهدف مبدئي
        currentTargetType = TargetType.Castle;
        detectedDefenses.Clear();

        Debug.Log($"<color=cyan>[Detection Setup]</color> {gameObject.name} initialized with Castle: {castleTransform.name}");
    }

    private void Update()
    {
        if (enemyController == null || enemyController.IsDead || !agent.enabled) return;

        // فحص مستمر بالأوفرلاب للتأكد من إلتقاط أي دفاع داخل النطاق
        ScanForDefenses();
        
        EvaluateTargets();
        UpdateMovementDestination();
    }

    private void ScanForDefenses()
    {
        // حساب مركز وحجم مربع الاستشعار بالـ World Space
        Vector3 center = transform.TransformPoint(triggerBoxOffset);
        Vector3 halfExtents = triggerBoxSize / 2f;

        // جلب كل الكائنات داخل النطاق
        Collider[] hits = Physics.OverlapBox(center, halfExtents, transform.rotation, defenseLayer);

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
                Debug.Log($"<color=yellow>[Detected]</color> {gameObject.name} registered: <b>{other.gameObject.name}</b>");
            }
        }
    }

    private void EvaluateTargets()
    {
        // 1. تنظيف القائمة من الأهداف المدمرة
        for (int i = detectedDefenses.Count - 1; i >= 0; i--)
        {
            Transform t = detectedDefenses[i];
            if (t == null || !t.gameObject.activeInHierarchy || IsTargetDead(t))
            {
                detectedDefenses.RemoveAt(i);
            }
        }

        // 2. الثبات على الهدف الحالي إذا كان لا يزال حياً
        if (currentTarget != null && currentTargetType == TargetType.DefenseObject)
        {
            if (!IsTargetDead(currentTarget) && currentTarget.gameObject.activeInHierarchy)
            {
                return;
            }
            else
            {
                currentTarget = null;
                currentTargetType = TargetType.None;
            }
        }

        // 3. اختيار أقرب دفاع
        Transform closestDefense = GetClosestDefense();

        if (closestDefense != null)
        {
            if (currentTarget != closestDefense)
            {
                currentTarget = closestDefense;
                currentTargetType = TargetType.DefenseObject;
                Debug.Log($"<color=magenta>[Target Locked]</color> {gameObject.name} attacking defense: <b>{currentTarget.name}</b>");
            }
        }
        else
        {
            // 4. العودة إلى القلعة عند عدم وجود دفاعات
            if (currentTarget != ultimateTarget)
            {
                currentTarget = ultimateTarget;
                currentTargetType = TargetType.Castle;
            }
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
        Matrix4x4 localMatrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
        Gizmos.matrix = localMatrix;
        Gizmos.DrawWireCube(triggerBoxOffset, triggerBoxSize);
    }
}