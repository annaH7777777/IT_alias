using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CategoryUiController : MonoBehaviour
{
    //[SerializeField] Dropdown categoryDropdown;
    [SerializeField] private TMP_Dropdown categoryDropdown;
    [SerializeField] Button startButton;
    
    
    void Start()
    {
        //categoryDropdown.ClearOptions();
        startButton.onClick.AddListener(OnStartClicked);
    }

    private void OnDestroy()
    {
        startButton.onClick.RemoveAllListeners();
    }

    private void OnStartClicked()
    {
        if (categoryDropdown.value == 0)
        {
            SceneManager.LoadScene(2);
            Debug.Log("Selected practice mode");
        }
            
        else
        {
            SceneManager.LoadScene(3);
            Debug.Log("Selected challenge mode");
        }
    }
}
