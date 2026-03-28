using System.Collections.Generic;
using UnityEngine;

public static class RoundedRect
{
    private static readonly Dictionary<int, Sprite> _cache = new Dictionary<int, Sprite>();

    public static Sprite Create(int width, int height, int radius)
    {
        int key = width * 10000 + height * 100 + radius;
        if (_cache.TryGetValue(key, out Sprite cached))
            return cached;

        var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;

        Color transparent = new Color(1, 1, 1, 0);
        Color white = Color.white;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float alpha = GetRoundedRectAlpha(x, y, width, height, radius);
                tex.SetPixel(x, y, alpha > 0 ? new Color(1, 1, 1, alpha) : transparent);
            }
        }

        tex.Apply();

        var sprite = Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f,
            0, SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));

        _cache[key] = sprite;
        return sprite;
    }

    private static float GetRoundedRectAlpha(int x, int y, int w, int h, int r)
    {
        // Check each corner
        if (x < r && y < r)
            return CircleAlpha(x, y, r, r, r);
        if (x >= w - r && y < r)
            return CircleAlpha(x, y, w - r - 1, r, r);
        if (x < r && y >= h - r)
            return CircleAlpha(x, y, r, h - r - 1, r);
        if (x >= w - r && y >= h - r)
            return CircleAlpha(x, y, w - r - 1, h - r - 1, r);

        return 1f;
    }

    private static float CircleAlpha(int x, int y, int cx, int cy, int r)
    {
        float dx = x - cx;
        float dy = y - cy;
        float dist = Mathf.Sqrt(dx * dx + dy * dy);

        if (dist <= r - 1f) return 1f;
        if (dist >= r + 1f) return 0f;
        return Mathf.Clamp01(r - dist + 1f); // anti-aliased edge
    }
}
