using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyController))]
public class HitboxController : MonoBehaviour
{
    private EnemyController enemyController;
    private readonly List<EnemyHitbox> registeredHitboxes = new List<EnemyHitbox>();

    // حقول حالة للتحكم في الزحف والإصابات مستقبلاً
    public bool IsLegDisabled { get; private set; }
    public bool IsArmDisabled { get; private set; }

    private void Awake()
    {
        enemyController = GetComponent<EnemyController>();
        
        if (enemyController == null)
        {
            Debug.LogError($"[HitboxController] Missing required EnemyController on {gameObject.name}!", this);
        }
    }

    /// <summary>
    /// يتم استدعاؤها تلقائياً من كل EnemyHitbox للتسجيل في القائمة
    /// </summary>
    public void RegisterHitbox(EnemyHitbox hitbox)
    {
        if (!registeredHitboxes.Contains(hitbox))
        {
            registeredHitboxes.Add(hitbox);
        }
    }

    /// <summary>
    /// معالجة الضرر والنتائج القادمة من الأجزاء المختلفة
    /// </summary>
    public void ProcessDamage(EnemyHitbox hitbox, HitInfo info)
    {
        if (enemyController == null || enemyController.IsDead) return;

        HitboxData data = hitbox.Data;

        // 1. القتل الفوري (مثلاً ضربة في الرأس)
        if (data.IsInstantKill)
        {
            Debug.Log($"<color=red>[HITBOX] INSTANT KILL via {hitbox.gameObject.name}!</color>");
            enemyController.TakeDamage(enemyController.MaxHealth);
            return;
        }

        // 2. حساب الضرر النهائي بناءً على الـ Multiplier
        float calculatedDamage = info.BaseDamage * data.DamageMultiplier;

        // 3. تقييم تأثير الجزء المصاب (الساق / اليد)
        EvaluateBodyPartEffects(data.PartType, calculatedDamage);

        Debug.Log($"<color=orange>[HITBOX] Hit {data.PartType}! Base: {info.BaseDamage} | Multiplier: {data.DamageMultiplier} | Final: {calculatedDamage}</color>");

        // 4. إرسال الضرر النهائي للـ EnemyController الرئيسي
        enemyController.TakeDamage(calculatedDamage);
    }

    private void EvaluateBodyPartEffects(BodyPartType partType, float damageTaken)
    {
        switch (partType)
        {
            case BodyPartType.Limb_Leg:
                // نقطة الربط القادمة لنظام الساق والتسبب في الزحف (Crawling)
                IsLegDisabled = true;
                break;

            case BodyPartType.Limb_Arm:
                IsArmDisabled = true;
                break;
        }
    }

    public void ToggleAllHitboxes(bool state)
    {
        for (int i = 0; i < registeredHitboxes.Count; i++)
        {
            if (registeredHitboxes[i] != null)
            {
                registeredHitboxes[i].SetColliderState(state);
            }
        }
    }
}