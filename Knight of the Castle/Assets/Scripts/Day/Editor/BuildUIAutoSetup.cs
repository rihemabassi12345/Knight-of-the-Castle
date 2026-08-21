#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuildUIAutoSetup : Editor
{
    [MenuItem("Tools/Build System/Create Build UI Canvas")]
    public static void CreateBuildUI()
    {
        // 1. Create Main Canvas
        GameObject canvasGO = new GameObject("BuildCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        // 2. Create Panel Container
        GameObject panel = new GameObject("BuildPanel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(canvasGO.transform, false);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0f);
        panelRect.anchorMax = new Vector2(0.5f, 0f);
        panelRect.pivot = new Vector2(0.5f, 0f);
        panelRect.sizeDelta = new Vector2(800, 150);
        panelRect.anchoredPosition = new Vector2(0, 20);

        Image panelImg = panel.GetComponent<Image>();
        panelImg.color = new Color(0f, 0f, 0f, 0.6f);

        // 3. Create Horizontal Layout Group Container
        GameObject container = new GameObject("ButtonContainer", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        container.transform.SetParent(panel.transform, false);
        RectTransform containerRect = container.GetComponent<RectTransform>();
        containerRect.anchorMin = Vector2.zero;
        containerRect.anchorMax = Vector2.one;
        containerRect.sizeDelta = Vector2.zero;

        HorizontalLayoutGroup layout = container.GetComponent<HorizontalLayoutGroup>();
        layout.spacing = 15;
        layout.padding = new RectOffset(10, 10, 10, 10);
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = false;

        // 4. Create Button Prefab Template
        GameObject buttonPrefab = new GameObject("BuildButtonTemplate", typeof(RectTransform), typeof(Image), typeof(Button));
        RectTransform btnRect = buttonPrefab.GetComponent<RectTransform>();
        btnRect.sizeDelta = new Vector2(120, 120);

        // Add Icon Image
        GameObject iconGO = new GameObject("IconImage", typeof(RectTransform), typeof(Image));
        iconGO.transform.SetParent(buttonPrefab.transform, false);
        RectTransform iconRect = iconGO.GetComponent<RectTransform>();
        iconRect.sizeDelta = new Vector2(50, 50);
        iconRect.anchoredPosition = new Vector2(0, 15);

        // Add Name Text
        GameObject nameGO = new GameObject("NameText", typeof(RectTransform), typeof(TextMeshProUGUI));
        nameGO.transform.SetParent(buttonPrefab.transform, false);
        RectTransform nameRect = nameGO.GetComponent<RectTransform>();
        nameRect.anchoredPosition = new Vector2(0, -25);
        nameRect.sizeDelta = new Vector2(110, 30);
        TMP_Text nameTmp = nameGO.GetComponent<TextMeshProUGUI>();
        nameTmp.text = "Item Name";
        nameTmp.fontSize = 12;
        nameTmp.alignment = TextAlignmentOptions.Center;

        // Add Cost Text
        GameObject costGO = new GameObject("CostText", typeof(RectTransform), typeof(TextMeshProUGUI));
        costGO.transform.SetParent(buttonPrefab.transform, false);
        RectTransform costRect = costGO.GetComponent<RectTransform>();
        costRect.anchoredPosition = new Vector2(0, -45);
        costRect.sizeDelta = new Vector2(110, 20);
        TMP_Text costTmp = costGO.GetComponent<TextMeshProUGUI>();
        costTmp.text = "15 Coins";
        costTmp.fontSize = 10;
        costTmp.color = Color.yellow;
        costTmp.alignment = TextAlignmentOptions.Center;

        // Attach UI Manager
        BuildUIManager uiManager = canvasGO.AddComponent<BuildUIManager>();

        // Save Button Template as a scene object / reference
        buttonPrefab.transform.SetParent(canvasGO.transform, false);
        buttonPrefab.SetActive(false);

        // Assign serialized fields using SerializedObject
        SerializedObject so = new SerializedObject(uiManager);
        so.FindProperty("buttonContainer").objectReferenceValue = container.transform;
        so.FindProperty("buttonPrefab").objectReferenceValue = buttonPrefab;
        so.ApplyModifiedProperties();

        Selection.activeGameObject = canvasGO;
        Debug.Log("Build UI Canvas generated automatically!");
    }
}
#endif