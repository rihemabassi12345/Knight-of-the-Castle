using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UpgradeDatabase", menuName = "Upgrades/Database")]
public class UpgradeDatabaseSO : ScriptableObject
{
    public List<UpgradeDataSO> allUpgrades = new List<UpgradeDataSO>();

    public List<UpgradeDataSO> GetRandomUniqueUpgrades(int count)
    {
        List<UpgradeDataSO> pool = new List<UpgradeDataSO>(allUpgrades);
        List<UpgradeDataSO> result = new List<UpgradeDataSO>();

        int amountToPick = Mathf.Min(count, pool.Count);
        for (int i = 0; i < amountToPick; i++)
        {
            int randomIndex = Random.Range(0, pool.Count);
            result.Add(pool[randomIndex]);
            pool.RemoveAt(randomIndex);
        }

        return result;
    }
}