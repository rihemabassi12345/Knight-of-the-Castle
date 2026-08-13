using UnityEngine;

public class CastleController : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth = 1000f;
    public float CurrentHealth { get; private set; }
    public bool IsDead { get; private set; }

    private void Awake()
    {
        CurrentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        if (IsDead) return;
        CurrentHealth -= damage;
        if (CurrentHealth <= 0) Die();
    }

    private void Die()
    {
        IsDead = true;
        // trigger lose-game logic here
    }
}