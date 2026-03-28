using System.Collections;
using UnityEngine;

/// <summary>
/// Fades in and slides up the attached UI element on Start.
/// NOTE: Do NOT use on elements inside Layout Groups — it modifies anchoredPosition.
/// </summary>
public class FadeInAnimator : MonoBehaviour
{
    [SerializeField] private float delay = 0f;
    [SerializeField] private float duration = 0.4f;
    [SerializeField] private float slideDistance = 30f;

    private CanvasGroup _canvasGroup;
    private RectTransform _rect;
    private Vector2 _targetPos;

    private void Start()
    {
        _rect = GetComponent<RectTransform>();
        _targetPos = _rect.anchoredPosition;
        _rect.anchoredPosition = _targetPos + Vector2.down * slideDistance;

        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();

        _canvasGroup.alpha = 0f;
        StartCoroutine(Animate());
    }

    private IEnumerator Animate()
    {
        if (delay > 0)
            yield return new WaitForSecondsRealtime(delay);

        float elapsed = 0f;
        Vector2 startPos = _rect.anchoredPosition;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            t = 1f - Mathf.Pow(1f - t, 3f);

            _canvasGroup.alpha = t;
            _rect.anchoredPosition = Vector2.Lerp(startPos, _targetPos, t);
            yield return null;
        }

        _canvasGroup.alpha = 1f;
        _rect.anchoredPosition = _targetPos;
    }
}
