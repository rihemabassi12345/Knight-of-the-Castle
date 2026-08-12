using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Enemy System/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Enemy Stats")]
    public float health;
    public float damage;
    public float speed;
    public float attackRange;
    public float detectionRange;

    [Header("Enemy Visuals")]
    public Sprite enemySprite;
    public Color enemyColor;

    [Header("Enemy Audio")]
    public AudioClip attackSound;
    public AudioClip deathSound;

    [Header("Enemy Behavior")]
    public bool canPatrol;
    public bool canChasePlayer;
}