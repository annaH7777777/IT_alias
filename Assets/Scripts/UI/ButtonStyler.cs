using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Applies color, shadow, and press animation to a button.
/// Safe with Layout Groups — does not modify size, position, or sprite.
/// </summary>
[RequireComponent(typeof(Button))]
public class ButtonStyler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private RectTransform _rect;
    private Vector3 _originalScale;
    private Color _baseColor = Color.white;
    private bool _colorSet;

    public void SetColor(Color color)
    {
        _baseColor = color;
        _colorSet = true;
    }

    private void Start()
    {
        _rect = GetComponent<RectTransform>();
        _originalScale = _rect.localScale;

        var button = GetComponent<Button>();
        var image = GetComponent<Image>();

        // Apply color
        if (_colorSet)
            image.color = _baseColor;

        // Disable default color tint so our color stays
        var cb = button.colors;
        cb.normalColor = Color.white;
        cb.highlightedColor = new Color(0.95f, 0.95f, 0.95f, 1f);
        cb.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
        cb.selectedColor = Color.white;
        _colorSet = true;
        button.colors = cb;

        // Style text based on button brightness
        var tmp = GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (tmp != null)
        {
            float brightness = (_baseColor.r + _baseColor.g + _baseColor.b) / 3f;
            tmp.color = brightness > 0.6f ? Theme.ButtonPrimaryText : Theme.TextWhite;
        }

        // Built-in shadow (no extra GameObjects)
        if (GetComponent<Shadow>() == null)
        {
            var shadow = gameObject.AddComponent<Shadow>();
            shadow.effectColor = Theme.CardShadow;
            shadow.effectDistance = new Vector2(0, -3);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        StopAllCoroutines();
        StartCoroutine(ScaleTo(Vector3.one * 0.93f, 0.08f));
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        StopAllCoroutines();
        StartCoroutine(ScaleTo(_originalScale, 0.15f));
    }

    private IEnumerator ScaleTo(Vector3 target, float duration)
    {
        Vector3 start = _rect.localScale;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            t = 1f - Mathf.Pow(1f - t, 3f);
            _rect.localScale = Vector3.Lerp(start, target, t);
            yield return null;
        }
        _rect.localScale = target;
    }
}
