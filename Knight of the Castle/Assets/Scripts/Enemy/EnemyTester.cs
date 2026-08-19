using UnityEngine;

public class EnemyTester : MonoBehaviour
{
    [Header("Target Enemy")]
    [SerializeField] private EnemyController targetEnemy;

    [Header("Damage Settings")]
    [SerializeField] private float damageAmount = 25f;

    private void Update()
    {
        if (targetEnemy == null) return;

        // اضغط على زر K في الكيبورد عشان تضرب العدو
        if (Input.GetKeyDown(KeyCode.K))
        {
            Debug.Log($"<color=yellow>[TEST] Dealing {damageAmount} damage to Enemy!</color>");
            targetEnemy.TakeDamage(damageAmount);
            Debug.Log($"<color=green>[TEST] Current Health: {targetEnemy.CurrentHealth} / {targetEnemy.MaxHealth}</color>");
        }

        // اضغط على زر L في الكيبورد عشان تموت العدو مرة وحدة
        if (Input.GetKeyDown(KeyCode.L))
        {
            Debug.Log("<color=red>[TEST] Killing Enemy instantly!</color>");
            targetEnemy.TakeDamage(9999f);
        }
    }
}