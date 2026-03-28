using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuUiController : MonoBehaviour
{
    [SerializeField] Button startButton;
    [SerializeField] Toggle sourceToggle;
    [SerializeField] CSVReader csvReader;
    [SerializeField] TextMeshProUGUI errorText;
    
    void Start()
    {
        startButton.onClick.AddListener(OnStartClicked);
    }

    private void OnDestroy()
    {
        startButton.onClick.RemoveAllListeners();
    }

    private void OnStartClicked()
    {
        bool readFromGoogle = sourceToggle.isOn;
        csvReader.StartLoading(readFromGoogle, b =>
        {
            if (b)
            {
                SceneManager.LoadScene("CategoryScene");
            }
            else
            {
                Debug.LogWarning("Loading from Google failed or was cancelled.");
                errorText.text = "Loading from Google failed or was cancelled.";
            }
        });
    }
}
