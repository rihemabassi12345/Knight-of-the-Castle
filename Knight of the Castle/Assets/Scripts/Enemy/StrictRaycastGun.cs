using UnityEngine;

public class StrictRaycastGun : MonoBehaviour
{
    [Header("Gun Configuration")]
    [SerializeField] private float baseDamage = 35f;
    [SerializeField] private float range = 100f;
    [SerializeField] private LayerMask targetLayers;

    private Camera mainCam;

    private void Awake()
    {
        mainCam = Camera.main;
        if (mainCam == null)
        {
            Debug.LogError("[StrictRaycastGun] Main Camera not found in scene!", this);
        }
    }

    private void Update()
    {
        // إطلاق عند الضغط على زر الماوس الأيسر
        if (Input.GetMouseButtonDown(0))
        {
            Fire();
        }
    }

    public void Fire()
    {
        if (mainCam == null) return;

        // خروج الشعاع من وسط الشاشة
        Ray ray = mainCam.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));

        if (Physics.Raycast(ray, out RaycastHit hit, range, targetLayers))
        {
            HitInfo info = new HitInfo
            {
                BaseDamage = baseDamage,
                HitPoint = hit.point,
                HitNormal = hit.normal,
                Attacker = transform
            };

            // 1. التجربة عبر نظام الـ Hitbox الجراحي (IHitbox Interface)
            IHitbox hitbox = hit.collider.GetComponent<IHitbox>();

            if (hitbox != null)
            {
                // رسم خط برتقالي عند إصابة أي Hitbox
                Color lineColour = hitbox.PartType == BodyPartType.Head ? Color.red : Color.orange;
                Debug.DrawLine(ray.origin, hit.point, lineColour, 1.5f);

                hitbox.OnHit(info);
            }
            else
            {
                // 2. تجربة الفالس مع الأهداف العادية (Castle / Obstacles)
                Debug.DrawLine(ray.origin, hit.point, Color.gray, 1.5f);

                IDamageable damageable = hit.collider.GetComponentInParent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(baseDamage);
                }
            }
        }
        else
        {
            // رسم خط أصفر متقطع إذا لم يصب أي شيء
            Debug.DrawLine(ray.origin, ray.direction * range, Color.yellow, 0.5f);
        }
    }
}