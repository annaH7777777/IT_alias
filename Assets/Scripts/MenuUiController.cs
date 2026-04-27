using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuUiController : MonoBehaviour
{
    [Serializable]
    public class WordList
    {
        public string displayName = "List";
        [TextArea] public string url = "https://docs.google.com/spreadsheets/d/e/your_id/pub?output=csv";
        [Tooltip("If on, show the category/date filter screen. If off, skip filters and play in Order-desc sequence.")]
        public bool hasFilters = true;
    }

    [SerializeField] Button startButton;
    [SerializeField] Button learnButton;
    [SerializeField] TMP_Dropdown wordListDropdown;
    [SerializeField] Toggle sourceToggle;
    [SerializeField] CSVReader csvReader;
    [SerializeField] TextMeshProUGUI errorText;

    [Header("Word Lists for Learn mode")]
    [SerializeField] List<WordList> wordLists = new List<WordList>();

    void Start()
    {
        startButton.onClick.AddListener(OnStartClicked);

        if (learnButton != null)
            learnButton.onClick.AddListener(OnLearnClicked);

        if (wordListDropdown == null && learnButton != null)
            wordListDropdown = BuildDropdownNear(learnButton);

        PopulateWordListDropdown();
    }

    private void OnDestroy()
    {
        startButton.onClick.RemoveAllListeners();
        if (learnButton != null)
            learnButton.onClick.RemoveAllListeners();
    }

    private void PopulateWordListDropdown()
    {
        if (wordListDropdown == null) return;
        wordListDropdown.ClearOptions();
        wordListDropdown.AddOptions(wordLists.Select(w => w.displayName).ToList());
        wordListDropdown.RefreshShownValue();
    }

    private void OnStartClicked()
    {
        bool readFromGoogle = sourceToggle.isOn;
        csvReader.StartLoading(readFromGoogle, b =>
        {
            if (b)
            {
                SceneManager.LoadScene("CategoryScene");
            }
            else
            {
                Debug.LogWarning("Loading from Google failed or was cancelled.");
                errorText.text = "Loading from Google failed or was cancelled.";
            }
        });
    }

    private void OnLearnClicked()
    {
        if (wordLists == null || wordLists.Count == 0)
        {
            errorText.text = "No word lists configured.";
            return;
        }

        int idx = wordListDropdown != null ? wordListDropdown.value : 0;
        if (idx < 0 || idx >= wordLists.Count) idx = 0;
        var selected = wordLists[idx];

        errorText.text = "Loading words...";

        LearningSheetReader.Instance.Load(selected.url, success =>
        {
            if (!success)
            {
                errorText.text = "Failed to load words.";
                return;
            }

            if (selected.hasFilters)
            {
                FilterController.FilteredWords = null;
            }
            else
            {
                FilterController.FilteredWords = LearningSheetReader.Instance.words
                    .OrderByDescending(w => w.order)
                    .ToList();
                FilterController.TranslationFirst = false;
            }

            SceneManager.LoadScene("FlashcardScene");
        });
    }

    // Builds a TMP_Dropdown as a sibling of the Learn button, positioned just above it.
    private TMP_Dropdown BuildDropdownNear(Button anchor)
    {
        var anchorRect = anchor.GetComponent<RectTransform>();
        var parent = (RectTransform)anchorRect.parent;

        var obj = new GameObject("WordListDropdown");
        obj.transform.SetParent(parent, false);

        var rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = anchorRect.anchorMin;
        rect.anchorMax = anchorRect.anchorMax;
        rect.pivot = anchorRect.pivot;
        rect.sizeDelta = new Vector2(anchorRect.sizeDelta.x, 70);
        rect.anchoredPosition = anchorRect.anchoredPosition + new Vector2(0, anchorRect.sizeDelta.y * 0.5f + 50);

        var image = obj.AddComponent<Image>();
        image.color = Theme.CardBackground;

        var dropdown = obj.AddComponent<TMP_Dropdown>();

        // Caption (selected value text)
        var labelObj = new GameObject("Label");
        labelObj.transform.SetParent(rect, false);
        var labelRect = labelObj.AddComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(20, 0);
        labelRect.offsetMax = new Vector2(-40, 0);
        var labelText = labelObj.AddComponent<TextMeshProUGUI>();
        labelText.fontSize = 30;
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
        arrowText.text = "▼";
        arrowText.fontSize = 24;
        arrowText.alignment = TextAlignmentOptions.Center;
        arrowText.color = Theme.TextDark;
        arrowText.raycastTarget = false;

        // Template (the popup list)
        var templateObj = new GameObject("Template");
        templateObj.transform.SetParent(rect, false);
        var templateRect = templateObj.AddComponent<RectTransform>();
        templateRect.anchorMin = new Vector2(0, 0);
        templateRect.anchorMax = new Vector2(1, 0);
        templateRect.pivot = new Vector2(0.5f, 1);
        templateRect.sizeDelta = new Vector2(0, 300);
        templateObj.AddComponent<Image>().color = Color.white;

        var scrollRect = templateObj.AddComponent<ScrollRect>();

        var viewportObj = new GameObject("Viewport");
        viewportObj.transform.SetParent(templateRect, false);
        var viewportRect = viewportObj.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = Vector2.zero;
        viewportObj.AddComponent<Image>().color = Color.white;
        viewportObj.AddComponent<Mask>().showMaskGraphic = false;

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
        itemRect.sizeDelta = new Vector2(0, 70);

        var itemToggle = itemObj.AddComponent<Toggle>();

        var itemBg = itemObj.AddComponent<Image>();
        itemBg.color = Color.white;

        var itemLabelObj = new GameObject("Item Label");
        itemLabelObj.transform.SetParent(itemRect, false);
        var itemLabelRect = itemLabelObj.AddComponent<RectTransform>();
        itemLabelRect.anchorMin = Vector2.zero;
        itemLabelRect.anchorMax = Vector2.one;
        itemLabelRect.offsetMin = new Vector2(20, 0);
        itemLabelRect.offsetMax = new Vector2(-20, 0);
        var itemLabel = itemLabelObj.AddComponent<TextMeshProUGUI>();
        itemLabel.fontSize = 28;
        itemLabel.alignment = TextAlignmentOptions.Left;
        itemLabel.color = Theme.TextDark;

        var checkObj = new GameObject("Item Checkmark");
        checkObj.transform.SetParent(itemRect, false);
        var checkRect = checkObj.AddComponent<RectTransform>();
        checkRect.anchorMin = new Vector2(1, 0.5f);
        checkRect.anchorMax = new Vector2(1, 0.5f);
        checkRect.sizeDelta = new Vector2(28, 28);
        checkRect.anchoredPosition = new Vector2(-26, 0);
        var checkImage = checkObj.AddComponent<Image>();
        checkImage.color = Theme.ButtonPrimaryText;

        itemToggle.graphic = checkImage;
        itemToggle.targetGraphic = itemBg;

        dropdown.template = templateRect;
        dropdown.itemText = itemLabel;

        templateObj.SetActive(false);

        return dropdown;
    }
}
