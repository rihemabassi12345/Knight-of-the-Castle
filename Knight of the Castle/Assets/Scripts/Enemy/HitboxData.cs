using UnityEngine;

public enum BodyPartType
{
    Head,
    Chest,
    Limb_Arm,
    Limb_Leg,
    WeakSpot
}

[System.Serializable]
public struct BoxColliderData// enrigestrement de different type de collider pour les hitbox
{
    public Vector3 center;
    public Vector3 size;
    public bool isTrigger;

    public static BoxColliderData Default => new BoxColliderData// direct set 
    {
        center = Vector3.zero,
        size = Vector3.one,
        isTrigger = false
    };
}

[CreateAssetMenu(fileName = "HitboxData_", menuName = "Combat System/Hitbox Data")]
public class HitboxData : ScriptableObject
{
    [Header("Core Settings")]
    [SerializeField] private BodyPartType partType = BodyPartType.Chest;
    [SerializeField, Range(0.1f, 10f)] private float damageMultiplier = 1.0f;
    [SerializeField] private bool isInstantKill = false;// die or no
    [SerializeField] private bool canBeSevered = false;// can be use off or not

    [Header("Box Collider Configuration")]
    [SerializeField] private bool overrideColliderData = true;
    [SerializeField] private BoxColliderData boxColliderSettings = BoxColliderData.Default;

    [Header("Impact Effects (VFX/SFX)")]
    [SerializeField] private GameObject hitVFXPrefab;
    [SerializeField] private AudioClip hitSound;

    // Encapsulated Read-only Properties
    public BodyPartType PartType => partType;
    public float DamageMultiplier => damageMultiplier;
    public bool IsInstantKill => isInstantKill;
    public bool CanBeSevered => canBeSevered;

    public bool OverrideColliderData => overrideColliderData;
    public BoxColliderData BoxColliderSettings => boxColliderSettings;

    public GameObject HitVFXPrefab => hitVFXPrefab;
    public AudioClip HitSound => hitSound;
}