using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class EnemyHitbox : MonoBehaviour, IHitbox
{
    [Header("Data Configuration")]
    [SerializeField] private HitboxData data;

    private BoxCollider cachedBoxCollider;
    public HitboxController ownerHub;

    // Encapsulated Read-only Properties
    public BodyPartType PartType => data != null ? data.PartType : BodyPartType.Chest;
    public HitboxData Data => data;

    private void Awake()
    {
        cachedBoxCollider = GetComponent<BoxCollider>();
        ApplyColliderData();
        ValidateSetup();
    }

    /// <summary>
    /// يطبق إعدادات الـ BoxCollider القادمة من الـ HitboxData تلقائياً
    /// </summary>
    public void ApplyColliderData()
    {
        if (data == null || !data.OverrideColliderData) return;

        if (cachedBoxCollider == null)
        {
            cachedBoxCollider = GetComponent<BoxCollider>();
        }

        if (cachedBoxCollider != null)
        {
            cachedBoxCollider.center = data.BoxColliderSettings.center;
            cachedBoxCollider.size = data.BoxColliderSettings.size;
            cachedBoxCollider.isTrigger = data.BoxColliderSettings.isTrigger;
        }
    }

    /// <summary>
    /// Strict Validation للتحقق من المراجع قبل تشغيل الكود لمنع الأخطاء
    /// </summary>
    private void ValidateSetup()
    {
        if (data == null)
        {
            Debug.LogError($"[Strict Hitbox Error] Missing HitboxData on {gameObject.name} in {transform.root.name}!", this);
        }

        ownerHub = GetComponentInParent<HitboxController>();
        if (ownerHub == null)
        {
            Debug.LogError($"[Strict Hitbox Error] EnemyHitbox on {gameObject.name} cannot find HitboxController in parent hierarchy!", this);
        }
        else
        {
            // تسجيل الـ Hitbox تلقائياً في الـ Central Hub
            ownerHub.RegisterHitbox(this);
        }
    }

    /// <summary>
    /// تطبيق العقد البرمجي IHitbox عند تلقي طلقة من السلاح
    /// </summary>
    public void OnHit(HitInfo info)
    {
        if (data == null || ownerHub == null)
        {
            Debug.LogError($"[Hitbox Failure] Attempted to hit uninitialized or invalid Hitbox: {gameObject.name}");
            return;
        }

        SpawnImpactEffects(info.HitPoint, info.HitNormal);
        
        // إرسال الضرر والتفاصيل للـ HitboxController المركزي
        ownerHub.ProcessDamage(this, info);
    }

    private void SpawnImpactEffects(Vector3 point, Vector3 normal)
    {
        if (data.HitVFXPrefab != null)
        {
            Instantiate(data.HitVFXPrefab, point, Quaternion.LookRotation(normal));
        }
    }

    public void SetColliderState(bool state)
    {
        if (cachedBoxCollider != null)
        {
            cachedBoxCollider.enabled = state;
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // مزامنة الأبعاد تلقائياً داخل الـ Unity Editor عند تغيير قيم ה- HitboxData
        if (data != null && data.OverrideColliderData)
        {
            ApplyColliderData();
        }
    }
#endif
}