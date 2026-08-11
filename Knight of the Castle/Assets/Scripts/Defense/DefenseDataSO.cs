using UnityEngine;

public enum DefenseRarity { Common, Rare, Epic, Legendary }

[CreateAssetMenu(fileName = "NewDefenseData", menuName = "Defenses/Defense Data")]
public class DefenseDataSO : ScriptableObject
{
    public string defenseName;
    public GameObject prefab;
    public Sprite icon;
    public int cost;
    public DefenseRarity rarity;
    public float maxHealth;
    public float damage;
    public float attackRange;
    public float attackRate;
}