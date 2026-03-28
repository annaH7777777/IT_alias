using UnityEngine;

public static class Theme
{
    // Background gradient
    public static readonly Color BackgroundTop = new Color32(30, 60, 114, 255);    // #1E3C72
    public static readonly Color BackgroundBottom = new Color32(69, 104, 170, 255); // #4568AA

    // Primary button
    public static readonly Color ButtonPrimary = new Color32(255, 255, 255, 255);
    public static readonly Color ButtonPrimaryText = new Color32(30, 60, 114, 255);

    // Level colors (A1–C2)
    public static readonly Color LevelA1 = new Color32(76, 175, 80, 255);   // green
    public static readonly Color LevelA2 = new Color32(102, 187, 106, 255); // light green
    public static readonly Color LevelB1 = new Color32(255, 179, 0, 255);   // amber
    public static readonly Color LevelB2 = new Color32(255, 152, 0, 255);   // orange
    public static readonly Color LevelC1 = new Color32(239, 83, 80, 255);   // red
    public static readonly Color LevelC2 = new Color32(198, 40, 40, 255);   // dark red

    // Text
    public static readonly Color TextWhite = Color.white;
    public static readonly Color TextDark = new Color32(33, 33, 33, 255);
    public static readonly Color TextMuted = new Color32(200, 210, 230, 255);

    // Card
    public static readonly Color CardBackground = new Color32(255, 255, 255, 230); // white, slight transparency
    public static readonly Color CardShadow = new Color32(0, 0, 0, 60);

    // Corner radius for generated sprites
    public const int CornerRadius = 24;
    public const int ButtonCornerRadius = 16;

    public static Color GetLevelColor(string level)
    {
        return level switch
        {
            "A1" => LevelA1,
            "A2" => LevelA2,
            "B1" => LevelB1,
            "B2" => LevelB2,
            "C1" => LevelC1,
            "C2" => LevelC2,
            _ => ButtonPrimary
        };
    }
}
