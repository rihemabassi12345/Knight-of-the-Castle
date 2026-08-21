using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuildUIManager : MonoBehaviour
{
    [Header("Data References")]
    [Tooltip("Drag your DefenseItemSO scriptable objects here.")]
    [SerializeField] private List<DefenseItemSO> availableDefenses;

    [Tooltip("Drag the GameObject with the BuildPhaseController script here.")]
    [SerializeField] private BuildPhaseController buildPhaseController;

    [Header("UI Component References")]
    [Tooltip("The HorizontalLayoutGroup object that will contain the buttons.")]
    [SerializeField] private Transform buttonContainer;

    [Tooltip("The disabled UI Button template prefab.")]
    [SerializeField] private GameObject buttonPrefab;

    private List<Button> spawnedButtons = new List<Button>();
    private List<DefenseItemSO> buttonData = new List<DefenseItemSO>();

    private void OnEnable()
    {
        GameEvents.OnCoinsChanged += RefreshButtonStates;
        GenerateUIButtons();
    }

    private void OnDisable()
    {
        GameEvents.OnCoinsChanged -= RefreshButtonStates;
    }

    private void GenerateUIButtons()
    {
        // 1. Safety Checks
        if (buttonContainer == null || buttonPrefab == null)
        {
            Debug.LogError("BuildUIManager: Missing Button Container or Button Prefab reference in the Inspector!");
            return;
        }

        if (availableDefenses == null || availableDefenses.Count == 0)
        {
            Debug.LogWarning("BuildUIManager: No DefenseItemSO assets added to the 'Available Defenses' list!");
            return;
        }

        // 2. Clear out any old buttons if we re-enable the UI
        foreach (Transform child in buttonContainer)
        {
            Destroy(child.gameObject);
        }

        spawnedButtons.Clear();
        buttonData.Clear();

        // 3. Get current coin count safely
        int currentCoins = CoinManager.Instance != null ? CoinManager.Instance.CurrentCoins : 0;

        // 4. Generate a button for each defense item in the list
        foreach (var item in availableDefenses)
        {
            if (item == null) continue;

            // Instantiate the template and parent it to the layout container
            GameObject newBtnObj = Instantiate(buttonPrefab, buttonContainer);

            // CRITICAL FIX: Ensure the cloned button is visible (since the template is usually hidden)
            newBtnObj.SetActive(true);

            Button btn = newBtnObj.GetComponent<Button>();

            // Find child UI components by their name (set up by the Editor script)
            TMP_Text nameText = newBtnObj.transform.Find("NameText")?.GetComponent<TMP_Text>();
            TMP_Text costText = newBtnObj.transform.Find("CostText")?.GetComponent<TMP_Text>();
            Image iconImage = newBtnObj.transform.Find("IconImage")?.GetComponent<Image>();

            // Setup texts and icons
            if (nameText != null) nameText.text = item.defenseName;
            if (costText != null) costText.text = $"{item.coinCost} Coins";
            if (iconImage != null && item.itemIcon != null) iconImage.sprite = item.itemIcon;

            // Hook up the button click event to select the building
            DefenseItemSO targetItem = item;
            btn.onClick.AddListener(() =>
            {
                if (buildPhaseController != null)
                {
                    buildPhaseController.SelectDefenseToBuild(targetItem.defensePrefab, targetItem.coinCost);
                }
                else
                {
                    Debug.LogError("BuildUIManager: BuildPhaseController is missing! Assign it in the Inspector.");
                }
            });

            // Set initial interactable state based on whether the player can afford it right now
            btn.interactable = currentCoins >= item.coinCost;

            // Track spawned buttons to update them later
            spawnedButtons.Add(btn);
            buttonData.Add(item);
        }
    }

    private void RefreshButtonStates(int newCoinBalance)
    {
        // This is called automatically when the player earns or spends coins
        for (int i = 0; i < spawnedButtons.Count; i++)
        {
            if (spawnedButtons[i] != null && buttonData[i] != null)
            {
                // Disable the button if the player is too broke to buy the item
                spawnedButtons[i].interactable = newCoinBalance >= buttonData[i].coinCost;
            }
        }
    }
}