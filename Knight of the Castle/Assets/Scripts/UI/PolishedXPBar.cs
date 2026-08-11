using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PolishedXPBar : MonoBehaviour
{
    [Header("UI References (Only 2 Required!)")]
    [SerializeField]  Slider xpSlider;
    [SerializeField]  TextMeshProUGUI levelText;

    [Header("XP Settings")]
    [SerializeField]  int currentLevel = 1;
    [SerializeField]  float currentXP = 0f;
    [SerializeField]  float targetMaxXP = 100f;
    [SerializeField]  float xpIncreasePerLevel = 1.2f; 

    [Header("Animation Settings")]
    [SerializeField]  float fillSpeed = 2.0f; 
    [SerializeField]  float punchScaleAmount = 1.15f;
    [SerializeField]  float punchDuration = 0.2f;

    private float animatedXP = 0f;
    private Coroutine fillCoroutine;
    private Vector3 originalSliderScale;

     void Start()
    {
        if (xpSlider != null)
            originalSliderScale = xpSlider.transform.localScale;

        UpdateUIImmediate();
    }

     void AddXP(float amount)
    {
        currentXP += amount;

        if (fillCoroutine != null)
            StopCoroutine(fillCoroutine);

        fillCoroutine = StartCoroutine(AnimateXPBar());
    }


    [ContextMenu("Test: Add 50 XP")]
    public void TestAdd50XP()
    {
        AddXP(50f);
    }

    [ContextMenu("Test: Add 150 XP (Level Up)")]
    public void TestAdd150XP()
    {
        AddXP(150f);
    }

    [ContextMenu("Test: Reset XP")]
    public void ResetXP()
    {
        if (fillCoroutine != null)
            StopCoroutine(fillCoroutine);

        currentLevel = 1;
        currentXP = 0f;
        animatedXP = 0f;
        targetMaxXP = 100f;

        UpdateUIImmediate();
    }

    IEnumerator AnimateXPBar()
    {
        while (currentXP >= targetMaxXP || Mathf.Abs(animatedXP - currentXP) > 0.01f)
        {
            // Level up condition
            if (animatedXP >= targetMaxXP)
            {
                OnLevelUp();
            }

            // Smoothly move the bar fill toward actual target XP
            animatedXP = Mathf.MoveTowards(animatedXP, currentXP, Time.deltaTime * targetMaxXP * fillSpeed);
            xpSlider.value = animatedXP / targetMaxXP;

            yield return null;
        }

        // Lock in final position
        animatedXP = currentXP;
        xpSlider.value = animatedXP / targetMaxXP;
    }

     void OnLevelUp()
    {
        currentXP -= targetMaxXP;
        animatedXP = 0f;

        currentLevel++;
        targetMaxXP *= xpIncreasePerLevel;

        if (levelText != null)
            levelText.text = $"LVL {currentLevel}";

        StartCoroutine(PunchScaleAnimation());
    }

     IEnumerator PunchScaleAnimation()
    {
        if (xpSlider == null) yield break;

        Vector3 targetScale = originalSliderScale * punchScaleAmount;
        float elapsed = 0f;

        while (elapsed < punchDuration / 2f)
        {
            elapsed += Time.deltaTime;
            xpSlider.transform.localScale = Vector3.Lerp(originalSliderScale, targetScale, elapsed / (punchDuration / 2f));
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < punchDuration / 2f)
        {
            elapsed += Time.deltaTime;
            xpSlider.transform.localScale = Vector3.Lerp(targetScale, originalSliderScale, elapsed / (punchDuration / 2f));
            yield return null;
        }

        xpSlider.transform.localScale = originalSliderScale;
    }

     void UpdateUIImmediate()
    {
        if (xpSlider != null)
            xpSlider.value = currentXP / targetMaxXP;

        if (levelText != null)
            levelText.text = $"LVL {currentLevel}";
    }
}