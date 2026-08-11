using UnityEngine;

public enum UpgradeType
{
    Health,
    WeaponDamage,
    MoveSpeed,
    AttackSpeed
}

[CreateAssetMenu(fileName = "NewUpgradeData", menuName = "Upgrades/Upgrade Data")]
public class UpgradeDataSO : ScriptableObject
{
    [Header("Basic Info")]
    public string upgradeName;
    public Sprite icon;
    public UpgradeType type;

    [Header("Base Scaling Settings")]
    public float baseValue = 20f;
    [Tooltip("Percentage increase per level (e.g., 0.15 = +15% per run/level)")]
    public float scalingFactorPerLevel = 0.15f;

    [TextArea]
    public string descriptionTemplate = "Increase {TYPE} by +{VALUE}";

    public float GetScaledValue(int currentLevel)
    {
        float multiplier = 1f + (scalingFactorPerLevel * Mathf.Max(0, currentLevel - 1));
        return Mathf.Round(baseValue * multiplier);
    }

    public string GetFormattedDescription(int currentLevel)
    {
        float scaledVal = GetScaledValue(currentLevel);
        return descriptionTemplate
            .Replace("{TYPE}", type.ToString())
            .Replace("{VALUE}", scaledVal.ToString("F0"));
    }
}