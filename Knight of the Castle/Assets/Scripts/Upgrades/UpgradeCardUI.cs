using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Button))]
public class UpgradeCardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Button selectButton;

    [Header("Animation Settings")]
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private float slideDuration = 0.35f;
    [SerializeField] private float offscreenYOffset = 1200f;

    [Header("Hover Scale Settings")]
    [SerializeField] private float hoverScaleFactor = 1.08f;
    [SerializeField] private float scaleSpeed = 12f;

    private UpgradeDataSO currentData;
    private float currentScaledValue;
    private Action<UpgradeCardUI, UpgradeDataSO, float> onSelectCallback;

    private Vector2 targetPosition;
    private Vector2 offscreenPosition;
    private Vector3 baseScale = Vector3.one;
    private Vector3 targetScale = Vector3.one;

    private Coroutine slideCoroutine;
    private Coroutine scaleCoroutine;

    private void Awake()
    {
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        if (selectButton == null) selectButton = GetComponent<Button>();

        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(OnCardClicked);

        // Cache initial scale setting
        baseScale = transform.localScale;
        targetScale = baseScale;
    }

    public void SetupCard(UpgradeDataSO data, int currentLevel, RectTransform containerSlot, Action<UpgradeCardUI, UpgradeDataSO, float> onSelected)
    {
        currentData = data;
        currentScaledValue = data.GetScaledValue(currentLevel);
        onSelectCallback = onSelected;

        // Populate Card UI
        if (iconImage != null) iconImage.sprite = data.icon;
        if (nameText != null) nameText.text = data.upgradeName;
        if (descriptionText != null) descriptionText.text = data.GetFormattedDescription(currentLevel);

        // Position according to assigned Container Slot
        targetPosition = containerSlot.anchoredPosition;
        offscreenPosition = new Vector2(targetPosition.x, targetPosition.y - offscreenYOffset);

        rectTransform.anchoredPosition = offscreenPosition;

        // Reset Card Scale safely to original layout scale
        targetScale = baseScale;
        transform.localScale = baseScale;
    }

    // --- SLIDE ANIMATIONS ---

    public void AnimateIn(float delay)
    {
        if (slideCoroutine != null) StopCoroutine(slideCoroutine);
        slideCoroutine = StartCoroutine(SlideRoutine(offscreenPosition, targetPosition, delay, null));
    }

    public void AnimateOut(float delay, Action onComplete)
    {
        if (slideCoroutine != null) StopCoroutine(slideCoroutine);

        // Reset scale before exiting offscreen
        SetHoverScaleState(false);
        slideCoroutine = StartCoroutine(SlideRoutine(rectTransform.anchoredPosition, offscreenPosition, delay, onComplete));
    }

    private IEnumerator SlideRoutine(Vector2 start, Vector2 end, float delay, Action onComplete)
    {
        yield return new WaitForSeconds(delay);

        float elapsed = 0f;
        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / slideDuration);
            rectTransform.anchoredPosition = Vector2.Lerp(start, end, t);
            yield return null;
        }

        rectTransform.anchoredPosition = end;
        onComplete?.Invoke();
    }

    // --- HOVER HANDLING ---

    public void OnPointerEnter(PointerEventData eventData)
    {
        SetHoverScaleState(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetHoverScaleState(false);
    }

    private void SetHoverScaleState(bool isHovered)
    {
        targetScale = isHovered ? baseScale * hoverScaleFactor : baseScale;

        if (gameObject.activeInHierarchy)
        {
            if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
            scaleCoroutine = StartCoroutine(ScaleRoutine());
        }
        else
        {
            transform.localScale = targetScale;
        }
    }

    private IEnumerator ScaleRoutine()
    {
        while (Vector3.Distance(transform.localScale, targetScale) > 0.001f)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * scaleSpeed);
            yield return null;
        }
        transform.localScale = targetScale;
    }

    private void OnCardClicked()
    {
        onSelectCallback?.Invoke(this, currentData, currentScaledValue);
    }
}