using UnityEngine;
using UnityEngine.AI;

// يجب أن يطبق IDamageable ليتعرف عليه نظام الهجوم الخاص بالعدو
[RequireComponent(typeof(NavMeshObstacle))]
public class ObstacleCube : MonoBehaviour, IDamageable
{
    [Header("Cube Stats")]
    [SerializeField] private float maxHealth = 100f;
    
    public float CurrentHealth { get; private set; }
    public bool IsDead { get; private set; }

    private NavMeshObstacle navObstacle;

    private void Awake()
    {
        navObstacle = GetComponent<NavMeshObstacle>();
        
        // تفعيل قطع الـ NavMesh لكي يلتف الأعداء أو يتوقفوا عنده
        if (navObstacle != null)
        {
            navObstacle.carving = true;
        }
    }

    private void OnEnable()
    {
        CurrentHealth = maxHealth;
        IsDead = false;
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

        // تعطيل الـ Obstacle لكي يفتح الطريق للأعداء فوراً بعد التدمير
        if (navObstacle != null)
        {
            navObstacle.enabled = false;
        }

        // إخفاء الكائن أو تدميره (أو إرجاعه للـ Pool)
        gameObject.SetActive(false);
    }
}