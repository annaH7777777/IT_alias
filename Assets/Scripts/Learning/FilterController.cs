using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Filter screen shown before flashcards. Lets user pick Category and/or Date.
/// Builds its own UI at runtime in FlashcardScene, then hands off to FlashcardController.
/// </summary>
public class FilterController : MonoBehaviour
{
    // Shared state: FlashcardController reads this after filter screen
    public static List<LearningWord> FilteredWords;

    private TMP_Dropdown _categoryDropdown;
    private TMP_Dropdown _dateDropdown;
    private TextMeshProUGUI _countText;
    private List<string> _categories;
    private List<string> _dates;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void OnSceneLoaded()
    {
        SceneManager.sceneLoaded += (scene, mode) =>
        {
            if (scene.name == "FlashcardScene" && FilteredWords == null)
                SetupFilterScreen();
        };

        if (SceneManager.GetActiveScene().name == "FlashcardScene" && FilteredWords == null)
            SetupFilterScreen();
    }

    private static void SetupFilterScreen()
    {
        if (FindObjectOfType<FilterController>() != null) return;
        if (FindObjectOfType<FlashcardController>() != null) return;

        var go = new GameObject("[FilterController]");
        go.AddComponent<FilterController>();
    }

    private void Start()
    {
        var reader = LearningSheetReader.Instance;
        if (reader.words == null || reader.words.Count == 0)
        {
            SceneManager.LoadScene("MenuScene");
            return;
        }

        _categories = reader.GetUniqueCategories();
        _dates = reader.GetUniqueDates();
        BuildUI();
        UpdateCount();
    }

    private void BuildUI()
    {
        var canvasObj = new GameObject("FilterCanvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 0;

        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 1f;

        canvasObj.AddComponent<GraphicRaycaster>();
        var canvasRect = canvasObj.GetComponent<RectTransform>();

        // Title
        var title = CreateText(canvasRect, "Title",
            new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1),
            new Vector2(40, -200), new Vector2(-40, -80),
            56, TextAlignmentOptions.Center, Theme.TextWhite);
        title.text = "Learn Words";
        title.fontStyle = FontStyles.Bold;

        // Category label
        CreateText(canvasRect, "CatLabel",
            new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1),
            new Vector2(80, -340), new Vector2(-80, -280),
            34, TextAlignmentOptions.Left, Theme.TextWhite).text = "Category:";

        // Category dropdown
        _categoryDropdown = CreateDropdown(canvasRect, "CategoryDropdown",
            new Vector2(80, -460), new Vector2(-80, -360));
        PopulateDropdown(_categoryDropdown, _categories, "All categories");
        _categoryDropdown.onValueChanged.AddListener(_ => UpdateCount());

        // Date label
        CreateText(canvasRect, "DateLabel",
            new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1),
            new Vector2(80, -560), new Vector2(-80, -500),
            34, TextAlignmentOptions.Left, Theme.TextWhite).text = "Date:";

        // Date dropdown
        _dateDropdown = CreateDropdown(canvasRect, "DateDropdown",
            new Vector2(80, -680), new Vector2(-80, -580));
        PopulateDropdown(_dateDropdown, _dates, "All dates");
        _dateDropdown.onValueChanged.AddListener(_ => UpdateCount());

        // Word count
        _countText = CreateText(canvasRect, "CountText",
            new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1),
            new Vector2(40, -800), new Vector2(-40, -740),
            36, TextAlignmentOptions.Center, Theme.TextMuted);

        // Start button
        var startBtn = CreateButton(canvasRect, "StartBtn", "Start Learning",
            new Vector2(0.15f, 0), new Vector2(0.85f, 0),
            new Vector2(0, 340), new Vector2(0, 440));
        startBtn.onClick.AddListener(OnStartLearning);

        // Back button
        var backBtn = CreateButton(canvasRect, "BackBtn", "Back",
            new Vector2(0.25f, 0), new Vector2(0.75f, 0),
            new Vector2(0, 220), new Vector2(0, 300));
        backBtn.onClick.AddListener(() => SceneManager.LoadScene("MenuScene"));
    }

    private void UpdateCount()
    {
        string cat = GetSelectedValue(_categoryDropdown, _categories);
        string date = GetSelectedValue(_dateDropdown, _dates);
        var filtered = LearningSheetReader.Instance.GetFilteredWords(cat, date);
        _countText.text = $"{filtered.Count} words found";
    }

    private void OnStartLearning()
    {
        string cat = GetSelectedValue(_categoryDropdown, _categories);
        string date = GetSelectedValue(_dateDropdown, _dates);
        var filtered = LearningSheetReader.Instance.GetFilteredWords(cat, date);

        if (filtered.Count == 0)
        {
            _countText.text = "No words match this filter!";
            return;
        }

        FilteredWords = filtered;
        // Destroy filter UI, FlashcardController will detect FilteredWords != null
        Destroy(gameObject);

        // Reload scene so FlashcardController picks up
        SceneManager.LoadScene("FlashcardScene");
    }

    private string GetSelectedValue(TMP_Dropdown dropdown, List<string> values)
    {
        // Index 0 = "All" (empty string = no filter)
        if (dropdown.value == 0) return "";
        return values[dropdown.value - 1];
    }

    private void PopulateDropdown(TMP_Dropdown dropdown, List<string> values, string allLabel)
    {
        dropdown.ClearOptions();
        var options = new List<string> { allLabel };
        options.AddRange(values);
        dropdown.AddOptions(options);
    }

    // --- UI Helpers ---

    private TextMeshProUGUI CreateText(RectTransform parent, string name,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
        Vector2 offsetMin, Vector2 offsetMax,
        float fontSize, TextAlignmentOptions alignment, Color color)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);

        var rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;

        var tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.fontSize = fontSize;
        tmp.alignment = alignment;
        tmp.color = color;
        tmp.enableWordWrapping = true;
        tmp.raycastTarget = false;

        return tmp;
    }

    private TMP_Dropdown CreateDropdown(RectTransform parent, string name, Vector2 offsetMin, Vector2 offsetMax)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);

        var rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(0.5f, 1);
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;

        var image = obj.AddComponent<Image>();
        image.color = Theme.CardBackground;

        var dropdown = obj.AddComponent<TMP_Dropdown>();

        // Label
        var labelObj = new GameObject("Label");
        labelObj.transform.SetParent(rect, false);
        var labelRect = labelObj.AddComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(20, 0);
        labelRect.offsetMax = new Vector2(-40, 0);
        var labelText = labelObj.AddComponent<TextMeshProUGUI>();
        labelText.fontSize = 32;
        labelText.alignment = TextAlignmentOptions.Left;
        labelText.color = Theme.TextDark;
        dropdown.captionText = labelText;

        // Arrow indicator
        var arrowObj = new GameObject("Arrow");
        arrowObj.transform.SetParent(rect, false);
        var arrowRect = arrowObj.AddComponent<RectTransform>();
        arrowRect.anchorMin = new Vector2(1, 0);
        arrowRect.anchorMax = new Vector2(1, 1);
        arrowRect.offsetMin = new Vector2(-50, 10);
        arrowRect.offsetMax = new Vector2(-10, -10);
        var arrowText = arrowObj.AddComponent<TextMeshProUGUI>();
        arrowText.text = "\u25BC";
        arrowText.fontSize = 24;
        arrowText.alignment = TextAlignmentOptions.Center;
        arrowText.color = Theme.TextDark;
        arrowText.raycastTarget = false;

        // Template (dropdown list)
        var templateObj = new GameObject("Template");
        templateObj.transform.SetParent(rect, false);
        var templateRect = templateObj.AddComponent<RectTransform>();
        templateRect.anchorMin = new Vector2(0, 0);
        templateRect.anchorMax = new Vector2(1, 0);
        templateRect.pivot = new Vector2(0.5f, 1);
        templateRect.offsetMin = new Vector2(0, 0);
        templateRect.offsetMax = new Vector2(0, 0);
        templateRect.sizeDelta = new Vector2(0, 400);

        var templateImage = templateObj.AddComponent<Image>();
        templateImage.color = Color.white;

        var scrollRect = templateObj.AddComponent<ScrollRect>();

        // Viewport
        var viewportObj = new GameObject("Viewport");
        viewportObj.transform.SetParent(templateRect, false);
        var viewportRect = viewportObj.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = Vector2.zero;
        viewportObj.AddComponent<Image>().color = Color.white;
        viewportObj.AddComponent<Mask>().showMaskGraphic = false;

        // Content
        var contentObj = new GameObject("Content");
        contentObj.transform.SetParent(viewportRect, false);
        var contentRect = contentObj.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.offsetMin = Vector2.zero;
        contentRect.offsetMax = Vector2.zero;

        scrollRect.viewport = viewportRect;
        scrollRect.content = contentRect;

        // Item template
        var itemObj = new GameObject("Item");
        itemObj.transform.SetParent(contentRect, false);
        var itemRect = itemObj.AddComponent<RectTransform>();
        itemRect.anchorMin = new Vector2(0, 0.5f);
        itemRect.anchorMax = new Vector2(1, 0.5f);
        itemRect.sizeDelta = new Vector2(0, 80);

        var itemToggle = itemObj.AddComponent<Toggle>();

        // Item background
        var itemBg = itemObj.AddComponent<Image>();
        itemBg.color = Color.white;

        // Item label
        var itemLabelObj = new GameObject("Item Label");
        itemLabelObj.transform.SetParent(itemRect, false);
        var itemLabelRect = itemLabelObj.AddComponent<RectTransform>();
        itemLabelRect.anchorMin = Vector2.zero;
        itemLabelRect.anchorMax = Vector2.one;
        itemLabelRect.offsetMin = new Vector2(20, 0);
        itemLabelRect.offsetMax = new Vector2(-20, 0);
        var itemLabel = itemLabelObj.AddComponent<TextMeshProUGUI>();
        itemLabel.fontSize = 30;
        itemLabel.alignment = TextAlignmentOptions.Left;
        itemLabel.color = Theme.TextDark;

        // Item checkmark
        var checkObj = new GameObject("Item Checkmark");
        checkObj.transform.SetParent(itemRect, false);
        var checkRect = checkObj.AddComponent<RectTransform>();
        checkRect.anchorMin = new Vector2(1, 0.5f);
        checkRect.anchorMax = new Vector2(1, 0.5f);
        checkRect.sizeDelta = new Vector2(30, 30);
        checkRect.anchoredPosition = new Vector2(-30, 0);
        var checkImage = checkObj.AddComponent<Image>();
        checkImage.color = Theme.ButtonPrimaryText;

        itemToggle.graphic = checkImage;
        itemToggle.targetGraphic = itemBg;

        dropdown.template = templateRect;
        dropdown.itemText = itemLabel;

        templateObj.SetActive(false);

        return dropdown;
    }

    private Button CreateButton(RectTransform parent, string name, string label,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);

        var rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0);
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;

        var image = obj.AddComponent<Image>();
        image.color = Color.white;

        var button = obj.AddComponent<Button>();

        var textObj = new GameObject("Text");
        textObj.transform.SetParent(rect, false);
        var textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        var tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 36;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Theme.ButtonPrimaryText;

        return button;
    }
}
