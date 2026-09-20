using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class GameBootstrap : MonoBehaviour
{
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private void Awake()
    {
        CreateEventSystemIfNeeded();

        Canvas canvas = CreateCanvas();
        CreateBackground(canvas.transform);
        CreateBoard(canvas.transform);
        CreateBackButton(canvas.transform);
    }

    private void CreateEventSystemIfNeeded()
    {
        EventSystem eventSystem = EventSystem.current;

        if (eventSystem == null)
        {
            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystem = eventSystemObject.AddComponent<EventSystem>();
        }

        StandaloneInputModule legacyInputModule = eventSystem.GetComponent<StandaloneInputModule>();
        if (legacyInputModule != null)
        {
            Destroy(legacyInputModule);
        }

        if (eventSystem.GetComponent<InputSystemUIInputModule>() == null)
        {
            eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
        }
    }

    private Canvas CreateCanvas()
    {
        GameObject canvasObject = new GameObject(
            "Canvas",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster));

        canvasObject.transform.SetParent(transform, false);

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        return canvas;
    }

    private void CreateBackground(Transform canvasTransform)
    {
        GameObject backgroundObject = new GameObject(
            "Background",
            typeof(RectTransform),
            typeof(Image));

        backgroundObject.transform.SetParent(canvasTransform, false);

        RectTransform backgroundRect = backgroundObject.GetComponent<RectTransform>();
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;

        Image background = backgroundObject.GetComponent<Image>();
        background.color = new Color32(24, 38, 48, 255);
    }

    private void CreateBoard(Transform canvasTransform)
    {
        GameObject boardObject = new GameObject(
            "Board",
            typeof(RectTransform),
            typeof(GridLayoutGroup));

        boardObject.transform.SetParent(canvasTransform, false);

        RectTransform boardRect = boardObject.GetComponent<RectTransform>();
        boardRect.anchorMin = new Vector2(0.5f, 0.5f);
        boardRect.anchorMax = new Vector2(0.5f, 0.5f);
        boardRect.pivot = new Vector2(0.5f, 0.5f);
        boardRect.sizeDelta = new Vector2(580f, 580f);
        boardRect.anchoredPosition = new Vector2(0f, 35f);

        GridLayoutGroup grid = boardObject.GetComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(180f, 180f);
        grid.spacing = new Vector2(20f, 20f);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 3;
        grid.childAlignment = TextAnchor.MiddleCenter;

        for (int index = 0; index < 9; index++)
        {
            CreateCell(boardObject.transform, index);
        }
    }

    private void CreateCell(Transform boardTransform, int index)
    {
        GameObject cellObject = new GameObject(
            $"Cell_{index + 1}",
            typeof(RectTransform),
            typeof(Image));

        cellObject.transform.SetParent(boardTransform, false);

        Image cell = cellObject.GetComponent<Image>();
        cell.color = new Color32(188, 198, 204, 255);
        cell.raycastTarget = false;
    }

    private void CreateBackButton(Transform canvasTransform)
    {
        GameObject buttonObject = new GameObject(
            "BackButton",
            typeof(RectTransform),
            typeof(Image),
            typeof(Button));

        buttonObject.transform.SetParent(canvasTransform, false);

        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.anchorMin = Vector2.zero;
        buttonRect.anchorMax = Vector2.zero;
        buttonRect.pivot = new Vector2(0.5f, 0.5f);
        buttonRect.sizeDelta = new Vector2(220f, 64f);
        buttonRect.anchoredPosition = new Vector2(140f, 60f);

        Image buttonImage = buttonObject.GetComponent<Image>();
        buttonImage.color = new Color32(70, 130, 190, 255);

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = buttonImage;
        button.onClick.AddListener(LoadMainMenuScene);

        GameObject labelObject = new GameObject(
            "Label",
            typeof(RectTransform),
            typeof(TextMeshProUGUI));

        labelObject.transform.SetParent(buttonObject.transform, false);

        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        TextMeshProUGUI label = labelObject.GetComponent<TextMeshProUGUI>();
        label.font = TMP_Settings.defaultFontAsset;
        label.text = "Volver al menú";
        label.fontSize = 24f;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;
    }

    private void LoadMainMenuScene()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
