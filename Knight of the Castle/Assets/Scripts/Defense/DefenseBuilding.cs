using UnityEngine;

public class DefenseBuilding : MonoBehaviour
{
    [SerializeField] protected DefenseDataSO data;
    protected float currentHealth;

    protected virtual void Start()
    {
        if (data != null) currentHealth = data.maxHealth;
    }

    public virtual void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }
}