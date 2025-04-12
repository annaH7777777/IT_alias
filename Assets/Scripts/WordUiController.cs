using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WordUiController : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI wordText;
    [SerializeField] TextMeshProUGUI hintText;
    [SerializeField] Button showHintButton;
    [SerializeField] Button backButton;
    [SerializeField] AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        showHintButton.onClick.AddListener(OnShowHint);
        backButton.onClick.AddListener(OnBackButtonClicked);
        hintText.gameObject.SetActive(false);
    }

    private void OnBackButtonClicked()
    {
        SceneManager.LoadScene(1);
    }

    private void OnShowHint()
    {
        hintText.gameObject.SetActive(true);
        audioSource.Play();
    }

    public void SetWord(Word word)
    {
        wordText.text = word.word;
        hintText.text = word.hint;
    }
}
