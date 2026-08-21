using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeathUIController : MonoBehaviour
{
    [Header("UI Panels & Canvas Groups")]
    [SerializeField] private CanvasGroup overlayCanvasGroup;
    [SerializeField] private Image blackOverlayImage;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI statusText;

    [Header("Heating Effect Settings")]
    [SerializeField] private Gradient heatColorGradient;
    [SerializeField] private float fadeDuration = 0.5f;

    [Header("Pulse Effect Settings")]
    [SerializeField] private float pulseScale = 1.3f;
    [SerializeField] private float pulseDuration = 0.25f;

    private Coroutine countdownCoroutine;
    private Coroutine pulseCoroutine;
    private Coroutine fadeCoroutine;
    private int lastDisplayedSecond = -1;
    private Vector3 originalTimerScale;

    private void Awake()
    {
        if (timerText != null)
        {
            originalTimerScale = timerText.transform.localScale;
        }

        // Initialize UI as hidden
        if (overlayCanvasGroup != null)
        {
            overlayCanvasGroup.alpha = 0f;
            overlayCanvasGroup.blocksRaycasts = false;
            overlayCanvasGroup.gameObject.SetActive(true); // Ensure object itself is enabled
        }

        if (blackOverlayImage != null)
        {
            Color c = Color.black;
            c.a = 0.3f;
            blackOverlayImage.color = c;
        }
    }

    public void ShowDeathUI(float duration, Action onTimerComplete = null)
    {
        // Stop any active countdown or fading routines before showing
        if (countdownCoroutine != null) StopCoroutine(countdownCoroutine);
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);

        // Bring panel to front of Canvas hierarchy so it isn't rendered behind other UI
        transform.SetAsLastSibling();

        countdownCoroutine = StartCoroutine(DeathSequenceRoutine(duration, onTimerComplete));
    }

    public void HideDeathUI()
    {
        if (countdownCoroutine != null) StopCoroutine(countdownCoroutine);
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeCanvasGroup(overlayCanvasGroup, overlayCanvasGroup.alpha, 0f, fadeDuration, false));
    }

    private IEnumerator DeathSequenceRoutine(float duration, Action onTimerComplete)
    {
        // Fade in panel
        fadeCoroutine = StartCoroutine(FadeCanvasGroup(overlayCanvasGroup, overlayCanvasGroup.alpha, 1f, fadeDuration, true));
        yield return fadeCoroutine;

        float timeRemaining = duration;
        lastDisplayedSecond = -1;

        while (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            int secondsLeft = Mathf.CeilToInt(timeRemaining);

            if (secondsLeft != lastDisplayedSecond && timerText != null)
            {
                lastDisplayedSecond = secondsLeft;
                timerText.text = secondsLeft.ToString();

                float progressRatio = 1f - (timeRemaining / duration);
                if (heatColorGradient != null)
                {
                    timerText.color = heatColorGradient.Evaluate(progressRatio);
                }

                TriggerPulse();
            }

            yield return null;
        }

        HideDeathUI();
        onTimerComplete?.Invoke();
    }

    public void TriggerPulse()
    {
        if (timerText == null) return;
        if (pulseCoroutine != null) StopCoroutine(pulseCoroutine);
        pulseCoroutine = StartCoroutine(PulseRoutine());
    }

    private IEnumerator PulseRoutine()
    {
        Transform targetTransform = timerText.transform;
        Vector3 peakScale = originalTimerScale * pulseScale;

        float elapsed = 0f;
        float halfDuration = pulseDuration * 0.5f;

        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            targetTransform.localScale = Vector3.Lerp(originalTimerScale, peakScale, elapsed / halfDuration);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            targetTransform.localScale = Vector3.Lerp(peakScale, originalTimerScale, elapsed / halfDuration);
            yield return null;
        }

        targetTransform.localScale = originalTimerScale;
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup group, float start, float end, float time, bool blockRaycasts)
    {
        if (group == null) yield break;

        group.blocksRaycasts = blockRaycasts;
        float elapsed = 0f;

        while (elapsed < time)
        {
            elapsed += Time.deltaTime;
            group.alpha = Mathf.Lerp(start, end, elapsed / time);
            yield return null;
        }

        group.alpha = end;
    }
}