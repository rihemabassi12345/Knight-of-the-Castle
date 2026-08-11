using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeManager : MonoBehaviour
{
    [Header("Database & UI Elements")]
    [SerializeField] private UpgradeDatabaseSO database;
    [SerializeField] private UpgradeCardUI[] cardSlots = new UpgradeCardUI[3];
    [SerializeField] private CanvasGroup uiCanvasGroup;

    [Header("Container Slot Locations")]
    [Tooltip("Drag empty UI RectTransforms here that define where each card should sit when active.")]
    [SerializeField] private RectTransform[] containerSlots = new RectTransform[3];

    [Header("Player Run State")]
    [SerializeField] private int currentLevel = 1;

    [Header("Testing & Flow Controls")]
    [Tooltip("If true, selecting an upgrade will dismiss the UI and automatically show new random upgrades.")]
    [SerializeField] private bool autoReappearAfterSelect = true;
    [SerializeField] private float delayBeforeReappear = 0.3f;

    private bool isAnimating = false;

    private void Start()
    {
        HideUIInstant();
    }

    #region Context Menu Utilities

    [ContextMenu("1. Simulate Next Level (Show UI)")]
    public void OnPlayerNextLevel()
    {
        if (isAnimating) return;

        currentLevel++;
        ShowUpgrades();
    }

    [ContextMenu("2. Select First Card (Simulate Click)")]
    public void SelectFirstCardDebug()
    {
        if (cardSlots.Length > 0 && cardSlots[0] != null && cardSlots[0].gameObject.activeSelf)
        {
            // Triggers the Button component directly on the card slot
            Button btn = cardSlots[0].GetComponentInChildren<Button>();
            if (btn != null)
            {
                btn.onClick.Invoke();
            }
            else
            {
                Debug.LogWarning("[UpgradeManager] No Button component found on Card 1.");
            }
        }
        else
        {
            Debug.LogWarning("[UpgradeManager] Cannot select card 1: Slot is inactive or empty.");
        }
    }

    [ContextMenu("3. Force Refresh Cards")]
    public void RefreshRandomUpgrades()
    {
        if (isAnimating) return;
        ShowUpgrades();
    }

    [ContextMenu("4. Instant Hide UI")]
    public void ForceHideUI()
    {
        StopAllCoroutines();
        isAnimating = false;
        HideUIInstant();
    }

    #endregion

    public void ShowUpgrades()
    {
        uiCanvasGroup.alpha = 1f;
        uiCanvasGroup.interactable = true;
        uiCanvasGroup.blocksRaycasts = true;

        List<UpgradeDataSO> selectedUpgrades = database.GetRandomUniqueUpgrades(cardSlots.Length);

        for (int i = 0; i < cardSlots.Length; i++)
        {
            if (i < selectedUpgrades.Count && i < containerSlots.Length)
            {
                cardSlots[i].gameObject.SetActive(true);

                // Pass the specific container slot RectTransform directly
                cardSlots[i].SetupCard(
                    selectedUpgrades[i],
                    currentLevel,
                    containerSlots[i],
                    OnUpgradeSelected
                );

                // Staggered slide up animation
                cardSlots[i].AnimateIn(i * 0.1f);
            }
            else
            {
                cardSlots[i].gameObject.SetActive(false);
            }
        }
    }

    private void OnUpgradeSelected(UpgradeCardUI clickedCard, UpgradeDataSO upgrade, float scaledValue)
    {
        if (isAnimating) return;
        StartCoroutine(DismissAndApplyRoutine(upgrade, scaledValue));
    }

    private IEnumerator DismissAndApplyRoutine(UpgradeDataSO upgrade, float scaledValue)
    {
        isAnimating = true;
        uiCanvasGroup.interactable = false; // Prevent multiple clicks during exit animation

        int completedCount = 0;
        int activeSlots = 0;

        // Slide cards back down off-screen
        for (int i = 0; i < cardSlots.Length; i++)
        {
            if (cardSlots[i] != null && cardSlots[i].gameObject.activeSelf)
            {
                activeSlots++;
                cardSlots[i].AnimateOut(i * 0.08f, () => completedCount++);
            }
        }

        // Wait until all cards completely finish sliding down
        yield return new WaitUntil(() => completedCount >= activeSlots);

        ApplyUpgradeToPlayer(upgrade.type, scaledValue);

        HideUIInstant();
        isAnimating = false;

        // Automatically reappear with new random cards if enabled
        if (autoReappearAfterSelect)
        {
            yield return new WaitForSeconds(delayBeforeReappear);
            OnPlayerNextLevel();
        }
    }

    private void ApplyUpgradeToPlayer(UpgradeType type, float value)
    {
        Debug.Log($"[Run Level {currentLevel}] Upgraded {type} by +{value}!");
    }

    private void HideUIInstant()
    {
        uiCanvasGroup.alpha = 0f;
        uiCanvasGroup.interactable = false;
        uiCanvasGroup.blocksRaycasts = false;

        foreach (var slot in cardSlots)
        {
            if (slot != null)
            {
                slot.gameObject.SetActive(false);
            }
        }
    }
}