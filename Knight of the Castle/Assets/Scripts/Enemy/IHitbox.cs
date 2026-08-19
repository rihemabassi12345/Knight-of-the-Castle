using UnityEngine;

/// <summary>
/// Struct تحتوي على كافة بيانات الضرب الموجهة للجزء المصاب.
/// </summary>
public struct HitInfo
{
    public float BaseDamage;
    public Vector3 HitPoint;
    public Vector3 HitNormal;
    public Transform Attacker;
}

/// <summary>
/// Contract برمجي صارم يضمن أن أي Hitbox يقبل استقبال الضربات بأسلوب موحد.
/// </summary>
public interface IHitbox
{
    BodyPartType PartType { get; }
    void OnHit(HitInfo info);
}