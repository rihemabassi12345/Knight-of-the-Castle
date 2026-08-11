using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DefensePlacementManager : MonoBehaviour
{
    [SerializeField] private Transform slotContainer;
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private LayerMask placementLayerMask;

    private DefenseDataSO selectedDefenseToPlace;

    private void OnEnable()
    {
        GenerateRandomSlots();
    }

    public void GenerateRandomSlots()
    {
        foreach (Transform child in slotContainer)
        {
            Destroy(child.gameObject);
        }

        var database = DayNightManager.Instance != null ? DayNightManager.Instance.GetDatabase() : null;
        if (database == null) return;

        List<DefenseDataSO> currentSlots = database.GetRandomDefensesForLevel(4, DayNightManager.Instance.GetCurrentDay());

        foreach (var data in currentSlots)
        {
            GameObject newSlot = Instantiate(slotPrefab, slotContainer);
            Image icon = newSlot.transform.Find("Icon")?.GetComponent<Image>();
            Button btn = newSlot.GetComponent<Button>();

            if (icon != null) icon.sprite = data.icon;
            if (btn != null) btn.onClick.AddListener(() => SelectDefense(data));
        }
    }

    public void SelectDefense(DefenseDataSO data)
    {
        if (!DayNightManager.Instance.CanPlaceDefenses())
        {
            Debug.Log("Cannot place defenses outside Prep Phase!");
            return;
        }
        selectedDefenseToPlace = data;
    }

    private void Update()
    {
        if (selectedDefenseToPlace == null || !DayNightManager.Instance.CanPlaceDefenses()) return;

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, placementLayerMask))
            {
                if (DayNightManager.Instance.SpendCoins(selectedDefenseToPlace.cost))
                {
                    Instantiate(selectedDefenseToPlace.prefab, hit.point, Quaternion.identity);
                    selectedDefenseToPlace = null;
                }
            }
        }
    }
}