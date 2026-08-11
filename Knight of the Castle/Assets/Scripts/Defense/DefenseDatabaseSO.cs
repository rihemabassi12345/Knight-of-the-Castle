using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DefenseDatabase", menuName = "Defenses/Defense Database")]
public class DefenseDatabaseSO : ScriptableObject
{
    public List<DefenseDataSO> allDefenses;

    public List<DefenseDataSO> GetRandomDefensesForLevel(int count, int currentDay)
    {
        List<DefenseDataSO> selectedSlots = new List<DefenseDataSO>();
        List<DefenseDataSO> pool = new List<DefenseDataSO>();

        foreach (var defense in allDefenses)
        {
            if (currentDay < 3 && defense.rarity == DefenseRarity.Legendary) continue;
            pool.Add(defense);
        }

        for (int i = 0; i < count; i++)
        {
            if (pool.Count == 0) break;
            DefenseDataSO randomPick = pool[Random.Range(0, pool.Count)];
            selectedSlots.Add(randomPick);
        }

        return selectedSlots;
    }
}