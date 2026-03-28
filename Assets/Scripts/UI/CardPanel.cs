using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Creates a rounded, semi-transparent card panel behind child content.
/// Attach to a parent RectTransform to wrap its children in a card.
/// </summary>
public class CardPanel : MonoBehaviour
{
    [SerializeField] private Color cardColor = new Color32(255, 255, 255, 230);
    [SerializeField] private bool addShadow = true;

    private void Start()
    {
        var rect = GetComponent<RectTransform>();
        if (rect == null) return;

        // Card background
        var cardObj = new GameObject("CardBg");
        cardObj.transform.SetParent(transform, false);
        cardObj.transform.SetAsFirstSibling();

        var cardRect = cardObj.AddComponent<RectTransform>();
        cardRect.anchorMin = Vector2.zero;
        cardRect.anchorMax = Vector2.one;
        cardRect.offsetMin = new Vector2(-20, -20);
        cardRect.offsetMax = new Vector2(20, 20);

        var cardImage = cardObj.AddComponent<Image>();
        int w = Mathf.Max(64, (int)rect.rect.width + 40);
        int h = Mathf.Max(64, (int)rect.rect.height + 40);
        cardImage.sprite = RoundedRect.Create(w, h, Theme.CornerRadius);
        cardImage.type = Image.Type.Sliced;
        cardImage.color = cardColor;
        cardImage.raycastTarget = false;

        if (addShadow)
        {
            var shadowObj = new GameObject("CardShadow");
            shadowObj.transform.SetParent(transform, false);
            shadowObj.transform.SetAsFirstSibling();

            var shadowRect = shadowObj.AddComponent<RectTransform>();
            shadowRect.anchorMin = Vector2.zero;
            shadowRect.anchorMax = Vector2.one;
            shadowRect.offsetMin = new Vector2(-22, -26);
            shadowRect.offsetMax = new Vector2(22, 18);

            var shadowImage = shadowObj.AddComponent<Image>();
            shadowImage.sprite = RoundedRect.Create(w + 4, h + 4, Theme.CornerRadius + 2);
            shadowImage.type = Image.Type.Sliced;
            shadowImage.color = Theme.CardShadow;
            shadowImage.raycastTarget = false;
        }
    }
}
