using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour
{
    Button button;
    string level;

    void Start()
    {
        button = GetComponent<Button>();
        if (button == null)
        {
            Debug.LogError("Button component missing on " + gameObject.name);
            return;
        }

        var textComponent = GetComponentInChildren<TextMeshProUGUI>();
        if (textComponent == null)
        {
            Debug.LogError("TextMeshProUGUI not found in children of " + gameObject.name);
            return;
        }

        level = textComponent.text;
        button.onClick.AddListener(OnClick);
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(OnClick);
    }

    private void OnClick()
    {
        PlayerPrefs.SetString("Level", level);
    }
}
