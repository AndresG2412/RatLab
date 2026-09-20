using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class MainMenuBootstrap : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "Game";

    private void Awake()
    {
        CreateEventSystemIfNeeded();

        Canvas canvas = CreateCanvas();
        CreateBackground(canvas.transform);
        CreateTitle(canvas.transform);
        CreatePlayButton(canvas.transform);
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
        background.color = new Color32(18, 21, 30, 255);
    }

    private void CreateTitle(Transform canvasTransform)
    {
        GameObject titleObject = new GameObject(
            "Title",
            typeof(RectTransform),
            typeof(TextMeshProUGUI));

        titleObject.transform.SetParent(canvasTransform, false);

        RectTransform titleRect = titleObject.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.5f);
        titleRect.anchorMax = new Vector2(0.5f, 0.5f);
        titleRect.pivot = new Vector2(0.5f, 0.5f);
        titleRect.sizeDelta = new Vector2(900f, 160f);
        titleRect.anchoredPosition = new Vector2(0f, 220f);

        TextMeshProUGUI title = titleObject.GetComponent<TextMeshProUGUI>();
        title.font = TMP_Settings.defaultFontAsset;
        title.text = "RatLab";
        title.fontSize = 96f;
        title.fontStyle = FontStyles.Bold;
        title.alignment = TextAlignmentOptions.Center;
        title.color = Color.white;
    }

    private void CreatePlayButton(Transform canvasTransform)
    {
        GameObject buttonObject = new GameObject(
            "PlayButton",
            typeof(RectTransform),
            typeof(Image),
            typeof(Button));

        buttonObject.transform.SetParent(canvasTransform, false);

        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.pivot = new Vector2(0.5f, 0.5f);
        buttonRect.sizeDelta = new Vector2(280f, 80f);
        buttonRect.anchoredPosition = Vector2.zero;

        Image buttonImage = buttonObject.GetComponent<Image>();
        buttonImage.color = new Color32(70, 130, 190, 255);

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = buttonImage;
        button.onClick.AddListener(LoadGameScene);

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
        label.text = "Jugar";
        label.fontSize = 36f;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;
    }

    private void LoadGameScene()
    {
        SceneManager.LoadScene(gameSceneName);
    }
}
