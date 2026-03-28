using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Flashcard learning mode. Builds its own UI at runtime.
/// One word at a time: tap to flip (front=word, back=translation+details).
/// Prev/Next buttons to navigate.
/// </summary>
public class FlashcardController : MonoBehaviour
{
    private List<LearningWord> _words;
    private int _currentIndex;
    private bool _isFlipped;

    // UI references (created at runtime)
    private TextMeshProUGUI _wordText;
    private TextMeshProUGUI _pronunciationText;
    private TextMeshProUGUI _translationText;
    private TextMeshProUGUI _synonymText;
    private TextMeshProUGUI _dateText;
    private TextMeshProUGUI _frontDateText;
    private TextMeshProUGUI _progressText;
    private RectTransform _cardRect;
    private GameObject _frontSide;
    private GameObject _backSide;
    private CanvasGroup _cardCanvasGroup;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void OnSceneLoaded()
    {
        SceneManager.sceneLoaded += (scene, mode) =>
        {
            if (scene.name == "FlashcardScene")
                SetupScene();
        };

        if (SceneManager.GetActiveScene().name == "FlashcardScene")
            SetupScene();
    }

    private static void SetupScene()
    {
        if (FindObjectOfType<FlashcardController>() != null) return;

        var go = new GameObject("[FlashcardController]");
        go.AddComponent<FlashcardController>();
    }

    private void Start()
    {
        var reader = LearningSheetReader.Instance;
        _words = reader.words;

        if (_words == null || _words.Count == 0)
        {
            Debug.LogError("No learning words loaded");
            SceneManager.LoadScene("MenuScene");
            return;
        }

        _currentIndex = 0;
        _isFlipped = false;
        BuildUI();
        ShowCard();
    }

    private void BuildUI()
    {
        // Canvas
        var canvasObj = new GameObject("FlashcardCanvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 0;

        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 1f;

        canvasObj.AddComponent<GraphicRaycaster>();

        var canvasRect = canvasObj.GetComponent<RectTransform>();

        // === Progress text (top) ===
        _progressText = CreateText(canvasRect, "ProgressText",
            anchorMin: new Vector2(0, 1), anchorMax: new Vector2(1, 1),
            pivot: new Vector2(0.5f, 1),
            offsetMin: new Vector2(40, -120), offsetMax: new Vector2(-40, -40),
            fontSize: 36, alignment: TextAlignmentOptions.Center,
            color: Theme.TextMuted);

        // === Card area (tap to flip) ===
        var cardObj = new GameObject("Card");
        cardObj.transform.SetParent(canvasRect, false);
        _cardRect = cardObj.AddComponent<RectTransform>();
        _cardRect.anchorMin = new Vector2(0.05f, 0.25f);
        _cardRect.anchorMax = new Vector2(0.95f, 0.78f);
        _cardRect.offsetMin = Vector2.zero;
        _cardRect.offsetMax = Vector2.zero;

        var cardImage = cardObj.AddComponent<Image>();
        cardImage.color = Theme.CardBackground;

        var shadow = cardObj.AddComponent<Shadow>();
        shadow.effectColor = Theme.CardShadow;
        shadow.effectDistance = new Vector2(0, -4);

        _cardCanvasGroup = cardObj.AddComponent<CanvasGroup>();

        var cardButton = cardObj.AddComponent<Button>();
        cardButton.transition = Selectable.Transition.None;
        cardButton.onClick.AddListener(FlipCard);

        // --- Front side ---
        _frontSide = new GameObject("FrontSide");
        _frontSide.transform.SetParent(_cardRect, false);
        var frontRect = _frontSide.AddComponent<RectTransform>();
        frontRect.anchorMin = Vector2.zero;
        frontRect.anchorMax = Vector2.one;
        frontRect.offsetMin = new Vector2(40, 40);
        frontRect.offsetMax = new Vector2(-40, -40);

        _wordText = CreateText(frontRect, "WordText",
            anchorMin: new Vector2(0, 0.4f), anchorMax: new Vector2(1, 0.85f),
            pivot: new Vector2(0.5f, 0.5f),
            offsetMin: Vector2.zero, offsetMax: Vector2.zero,
            fontSize: 72, alignment: TextAlignmentOptions.Center,
            color: Theme.TextDark);
        _wordText.fontStyle = FontStyles.Bold;
        _wordText.enableAutoSizing = true;
        _wordText.fontSizeMin = 28;
        _wordText.fontSizeMax = 72;

        _pronunciationText = CreateText(frontRect, "PronunciationText",
            anchorMin: new Vector2(0, 0.2f), anchorMax: new Vector2(1, 0.4f),
            pivot: new Vector2(0.5f, 0.5f),
            offsetMin: Vector2.zero, offsetMax: Vector2.zero,
            fontSize: 36, alignment: TextAlignmentOptions.Center,
            color: new Color32(120, 120, 140, 255));

        _frontDateText = CreateText(frontRect, "FrontDateText",
            anchorMin: new Vector2(0, 0.1f), anchorMax: new Vector2(1, 0.2f),
            pivot: new Vector2(0.5f, 0.5f),
            offsetMin: Vector2.zero, offsetMax: Vector2.zero,
            fontSize: 26, alignment: TextAlignmentOptions.Center,
            color: Theme.TextMuted);

        var tapHint = CreateText(frontRect, "TapHint",
            anchorMin: new Vector2(0, 0.02f), anchorMax: new Vector2(1, 0.1f),
            pivot: new Vector2(0.5f, 0.5f),
            offsetMin: Vector2.zero, offsetMax: Vector2.zero,
            fontSize: 26, alignment: TextAlignmentOptions.Center,
            color: Theme.TextMuted);
        tapHint.text = "tap to flip";

        // --- Back side (hidden initially) ---
        _backSide = new GameObject("BackSide");
        _backSide.transform.SetParent(_cardRect, false);
        var backRect = _backSide.AddComponent<RectTransform>();
        backRect.anchorMin = Vector2.zero;
        backRect.anchorMax = Vector2.one;
        backRect.offsetMin = new Vector2(40, 40);
        backRect.offsetMax = new Vector2(-40, -40);
        _backSide.SetActive(false);

        _translationText = CreateText(backRect, "TranslationText",
            anchorMin: new Vector2(0, 0.55f), anchorMax: new Vector2(1, 0.85f),
            pivot: new Vector2(0.5f, 0.5f),
            offsetMin: Vector2.zero, offsetMax: Vector2.zero,
            fontSize: 52, alignment: TextAlignmentOptions.Center,
            color: Theme.TextDark);
        _translationText.fontStyle = FontStyles.Bold;
        _translationText.enableAutoSizing = true;
        _translationText.fontSizeMin = 24;
        _translationText.fontSizeMax = 52;

        _synonymText = CreateText(backRect, "SynonymText",
            anchorMin: new Vector2(0, 0.3f), anchorMax: new Vector2(1, 0.55f),
            pivot: new Vector2(0.5f, 0.5f),
            offsetMin: Vector2.zero, offsetMax: Vector2.zero,
            fontSize: 34, alignment: TextAlignmentOptions.Center,
            color: new Color32(100, 100, 120, 255));

        _dateText = CreateText(backRect, "DateText",
            anchorMin: new Vector2(0, 0.05f), anchorMax: new Vector2(1, 0.25f),
            pivot: new Vector2(0.5f, 0.5f),
            offsetMin: Vector2.zero, offsetMax: Vector2.zero,
            fontSize: 28, alignment: TextAlignmentOptions.Center,
            color: Theme.TextMuted);

        // === Navigation buttons (bottom) ===
        var navObj = new GameObject("Navigation");
        navObj.transform.SetParent(canvasRect, false);
        var navRect = navObj.AddComponent<RectTransform>();
        navRect.anchorMin = new Vector2(0, 0.08f);
        navRect.anchorMax = new Vector2(1, 0.2f);
        navRect.offsetMin = Vector2.zero;
        navRect.offsetMax = Vector2.zero;

        var navLayout = navObj.AddComponent<HorizontalLayoutGroup>();
        navLayout.spacing = 30;
        navLayout.childAlignment = TextAnchor.MiddleCenter;
        navLayout.childControlWidth = false;
        navLayout.childControlHeight = false;

        CreateNavButton(navRect, "Back", "\u25C0  Back", 200, 80, () => SceneManager.LoadScene("MenuScene"));
        CreateNavButton(navRect, "Prev", "\u25C0  Prev", 200, 80, PrevCard);
        CreateNavButton(navRect, "Next", "Next  \u25B6", 200, 80, NextCard);
    }

    private void ShowCard()
    {
        if (_words.Count == 0) return;

        var word = _words[_currentIndex];

        _wordText.text = word.word;
        _pronunciationText.text = string.IsNullOrEmpty(word.pronunciation) ? "" : word.pronunciation;

        _translationText.text = string.IsNullOrEmpty(word.translation) ? "(no translation)" : word.translation;
        _synonymText.text = string.IsNullOrEmpty(word.synonym) ? "" : $"syn: {word.synonym}";
        _dateText.text = string.IsNullOrEmpty(word.date) ? "" : word.date;

        _frontDateText.text = string.IsNullOrEmpty(word.date) ? "" : word.date;
        _progressText.text = $"{_currentIndex + 1} / {_words.Count}";

        // Reset to front side
        _isFlipped = false;
        _frontSide.SetActive(true);
        _backSide.SetActive(false);

        // Quick fade-in
        StartCoroutine(CardFadeIn());
    }

    private IEnumerator CardFadeIn()
    {
        _cardCanvasGroup.alpha = 0f;
        float elapsed = 0f;
        while (elapsed < 0.2f)
        {
            elapsed += Time.unscaledDeltaTime;
            _cardCanvasGroup.alpha = Mathf.Clamp01(elapsed / 0.2f);
            yield return null;
        }
        _cardCanvasGroup.alpha = 1f;
    }

    private void FlipCard()
    {
        _isFlipped = !_isFlipped;
        StartCoroutine(FlipAnimation());
    }

    private IEnumerator FlipAnimation()
    {
        float duration = 0.25f;
        float elapsed = 0f;
        Vector3 startScale = _cardRect.localScale;

        // Shrink horizontally
        while (elapsed < duration / 2f)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / (duration / 2f);
            _cardRect.localScale = new Vector3(Mathf.Lerp(1f, 0f, t), 1f, 1f);
            yield return null;
        }

        // Swap sides
        _frontSide.SetActive(!_isFlipped);
        _backSide.SetActive(_isFlipped);

        // Expand horizontally
        elapsed = 0f;
        while (elapsed < duration / 2f)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / (duration / 2f);
            _cardRect.localScale = new Vector3(Mathf.Lerp(0f, 1f, t), 1f, 1f);
            yield return null;
        }

        _cardRect.localScale = Vector3.one;
    }

    private void NextCard()
    {
        _currentIndex = (_currentIndex + 1) % _words.Count;
        ShowCard();
    }

    private void PrevCard()
    {
        _currentIndex = (_currentIndex - 1 + _words.Count) % _words.Count;
        ShowCard();
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

    private void CreateNavButton(RectTransform parent, string name, string label, float width, float height, UnityEngine.Events.UnityAction onClick)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);

        var rect = obj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(width, height);

        var image = obj.AddComponent<Image>();
        image.color = Color.white;

        var button = obj.AddComponent<Button>();
        button.onClick.AddListener(onClick);

        var textObj = new GameObject("Text");
        textObj.transform.SetParent(rect, false);

        var textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        var tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 30;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Theme.ButtonPrimaryText;
    }
}
