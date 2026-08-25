using System;
using UnityEngine;

// 1. زيد IDamageable هنا بجانب IKillable
public class PlayerController : MonoBehaviour, IKillable, IDamageable
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 200f;
    [SerializeField] private float currentHealth;

    public float CurrentHealth => currentHealth;
    public bool IsDead { get; private set; }

    public event Action OnDied;
    public event Action OnRespawned;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        currentHealth = maxHealth;
    }

    // 2. تطبيق دالة TakeDamage الخاصة بالـ IDamageable
    public void TakeDamage(float damage)
    {
        if (IsDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0f);

        Debug.Log($"[King/Player] Took {damage} damage. Current Health: {currentHealth}");

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    public void Die()
    {
        if (IsDead) return;

        IsDead = true;
        if (rb != null) rb.isKinematic = true;
        OnDied?.Invoke();
    }

    public void ReviveAt(Vector3 position, Quaternion rotation)
    {
        transform.position = position;
        transform.rotation = rotation;
        currentHealth = maxHealth; // إعادة الصحة عند الـ Respawn
        IsDead = false;
        if (rb != null) rb.isKinematic = false;
        OnRespawned?.Invoke();
    }
}