using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Enemy System/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Health")]
    [Min(1f)]
    public float health = 100f;

    [Header("Combat")]
    [Min(0f)]
    public float damage = 10f;

    [Min(0.1f)]
    public float attackRange = 2f;

    [Min(0.1f)]
    public float attackCooldown = 1.5f;

    [Header("Movement")]
    [Min(0f)]
    public float speed = 3.5f;

    [Min(0f)]
    public float stoppingDistance = 1.5f;

    [Header("Detection")]
    [Min(0f)]
    public float detectionRange = 10f;

    [Header("Behavior Flags")]
    public bool canChasePlayer = true;
    public bool canAttackCastle = true;
    public bool canDestroyObstacles = true;
}