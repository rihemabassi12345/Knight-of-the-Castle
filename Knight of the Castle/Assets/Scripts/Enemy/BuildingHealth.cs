using UnityEngine;

public class BuildingHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private float health = 200f;
    public float CurrentHealth => health;
    public bool IsDead { get; private set; }

    public void TakeDamage(float damage)
    {
        if (IsDead) return;
        health -= damage;
        if (health <= 0)
        {
            IsDead = true;
            gameObject.SetActive(false); // أو أي logic لتدمير الحائط/الملك
        }
    }
}