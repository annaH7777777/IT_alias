using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour
{
    Button button;
    string level;
    
    // Start is called before the first frame update
    void Start()
    {
        button = GetComponent<Button>();
        level = GetComponentInChildren<TextMeshProUGUI>().text;
        button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        PlayerPrefs.SetString("Level", level);

    }
}
