using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
public class GradientBackground : MonoBehaviour
{
    private void Start()
    {
        ApplyGradient();
    }

    private void ApplyGradient()
    {
        // Create a background object behind all UI
        var bgObj = new GameObject("GradientBackground");
        bgObj.transform.SetParent(transform, false);
        bgObj.transform.SetAsFirstSibling();

        var rect = bgObj.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        var image = bgObj.AddComponent<RawImage>();

        var tex = new Texture2D(1, 2, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.SetPixel(0, 0, Theme.BackgroundBottom);
        tex.SetPixel(0, 1, Theme.BackgroundTop);
        tex.Apply();

        image.texture = tex;
        image.raycastTarget = false;

        // Disable camera clear so gradient shows
        var cam = Camera.main;
        if (cam != null)
            cam.clearFlags = CameraClearFlags.SolidColor;
    }
}
