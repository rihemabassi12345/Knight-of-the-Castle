using UnityEngine;

[CreateAssetMenu(fileName = "NewDefenseItem", menuName = "Building/Defense Item")]
public class DefenseItemSO : ScriptableObject
{
    public string defenseName;
    public int coinCost;
    public Sprite itemIcon;
    public GameObject defensePrefab;
}