using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class DeathUIEditorSetup : EditorWindow
{
    [MenuItem("Tools/UI/Auto Build Death UI Panel")]
    public static void CreateDeathUI()
    {
        // 1. Ensure Canvas exists
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            canvasObj.AddComponent<GraphicRaycaster>();
            Undo.RegisterCreatedObjectUndo(canvasObj, "Create Death UI Canvas");
        }

        // 2. Create Root Panel Container
        GameObject deathPanel = new GameObject("DeathUI_Panel", typeof(RectTransform), typeof(CanvasGroup));
        Undo.RegisterCreatedObjectUndo(deathPanel, "Create Death UI Panel");
        deathPanel.transform.SetParent(canvas.transform, false);

        RectTransform panelRect = deathPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        CanvasGroup canvasGroup = deathPanel.GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;

        // 3. Create 30% Black Overlay Image
        GameObject bgObj = new GameObject("BlackOverlayImage", typeof(RectTransform), typeof(Image));
        bgObj.transform.SetParent(deathPanel.transform, false);

        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        Image bgImage = bgObj.GetComponent<Image>();
        bgImage.color = new Color(0f, 0f, 0f, 0.3f); // 30% Opacity Black

        // 4. Create Container Layout
        GameObject contentContainer = new GameObject("ContentContainer", typeof(RectTransform), typeof(VerticalLayoutGroup));
        contentContainer.transform.SetParent(deathPanel.transform, false);

        RectTransform containerRect = contentContainer.GetComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.pivot = new Vector2(0.5f, 0.5f);
        containerRect.sizeDelta = new Vector2(800, 400);

        VerticalLayoutGroup layout = contentContainer.GetComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        layout.spacing = 10f;

        // 5. Create Status Text
        GameObject statusObj = new GameObject("StatusText", typeof(RectTransform), typeof(TextMeshProUGUI));
        statusObj.transform.SetParent(contentContainer.transform, false);

        TextMeshProUGUI statusText = statusObj.GetComponent<TextMeshProUGUI>();
        statusText.text = "YOU DIED\nRESPAWNING IN";
        statusText.fontSize = 36;
        statusText.fontStyle = FontStyles.Bold;
        statusText.alignment = TextAlignmentOptions.Center;
        statusText.color = Color.white;
        statusObj.GetComponent<RectTransform>().sizeDelta = new Vector2(800, 100);

        // 6. Create Countdown Timer Text
        GameObject timerObj = new GameObject("CountdownText", typeof(RectTransform), typeof(TextMeshProUGUI));
        timerObj.transform.SetParent(contentContainer.transform, false);

        TextMeshProUGUI timerText = timerObj.GetComponent<TextMeshProUGUI>();
        timerText.text = "20";
        timerText.fontSize = 96;
        timerText.fontStyle = FontStyles.Bold;
        timerText.alignment = TextAlignmentOptions.Center;
        timerText.color = new Color(1f, 0.2f, 0.1f);
        timerObj.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 150);

        // 7. Attach DeathUIController & Auto-Assign All Inspector Fields
        DeathUIController controller = deathPanel.AddComponent<DeathUIController>();

        SerializedObject serializedObj = new SerializedObject(controller);
        serializedObj.FindProperty("overlayCanvasGroup").objectReferenceValue = canvasGroup;
        serializedObj.FindProperty("blackOverlayImage").objectReferenceValue = bgImage;
        serializedObj.FindProperty("timerText").objectReferenceValue = timerText;
        serializedObj.FindProperty("statusText").objectReferenceValue = statusText;

        // Auto-Generate Default Heating Color Gradient
        SerializedProperty gradientProp = serializedObj.FindProperty("heatColorGradient");
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(1f, 0.2f, 0.1f), 0.0f),  // Red
                new GradientColorKey(new Color(1f, 0.8f, 0.0f), 0.5f),  // Gold Yellow
                new GradientColorKey(new Color(0.8f, 0.95f, 1f), 1.0f)  // Cool White
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1.0f, 0.0f),
                new GradientAlphaKey(1.0f, 1.0f)
            }
        );
        gradientProp.gradientValue = gradient;

        serializedObj.ApplyModifiedProperties();

        // Select the new UI object in Hierarchy
        Selection.activeGameObject = deathPanel;
        Debug.Log("<color=green>SUCCESS:</color> Death UI hierarchy, properties, and component references built and assigned automatically!");
    }
}