using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Auto-applies the modern theme to all UI elements when any scene loads.
/// Bootstraps itself via [RuntimeInitializeOnLoadMethod] — no manual setup needed.
/// </summary>
public class SceneStyler : MonoBehaviour
{
    private static SceneStyler _instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (_instance != null) return;

        var go = new GameObject("[SceneStyler]");
        _instance = go.AddComponent<SceneStyler>();
        DontDestroyOnLoad(go);

        _instance.StyleCurrentScene();
        SceneManager.sceneLoaded += (scene, mode) => _instance.StyleCurrentScene();
    }

    private void StyleCurrentScene()
    {
        // Gradient background on all canvases
        foreach (var canvas in FindObjectsOfType<Canvas>())
        {
            if (canvas.GetComponent<GradientBackground>() == null)
                canvas.gameObject.AddComponent<GradientBackground>();
        }

        // Style buttons
        foreach (var button in FindObjectsOfType<Button>(true))
        {
            if (button.GetComponent<ButtonStyler>() != null) continue;

            var styler = button.gameObject.AddComponent<ButtonStyler>();

            // Level buttons get colored by their text label
            var levelButton = button.GetComponent<LevelButton>();
            if (levelButton != null)
            {
                var tmp = button.GetComponentInChildren<TextMeshProUGUI>();
                if (tmp != null)
                    styler.SetColor(Theme.GetLevelColor(tmp.text.Trim()));
            }
        }

        // Standalone text → white for readability on gradient
        foreach (var tmp in FindObjectsOfType<TextMeshProUGUI>(true))
        {
            if (tmp.GetComponentInParent<Button>() != null) continue;
            if (tmp.GetComponentInParent<TMP_Dropdown>() != null) continue;
            if (tmp.GetComponentInParent<Toggle>() != null) continue;

            tmp.color = Theme.TextWhite;
        }

        // Dropdown styling (deferred for layout)
        StartCoroutine(StyleDropdownsDeferred());

        // Toggle labels → white
        foreach (var toggle in FindObjectsOfType<Toggle>(true))
        {
            var label = toggle.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
                label.color = Theme.TextWhite;
        }
    }

    private IEnumerator StyleDropdownsDeferred()
    {
        yield return null;

        foreach (var dropdown in FindObjectsOfType<TMP_Dropdown>(true))
        {
            var image = dropdown.GetComponent<Image>();
            if (image != null)
                image.color = Theme.CardBackground;
        }
    }
}
