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

    private void OnDestroy()
    {
        showHintButton.onClick.RemoveListener(OnShowHint);
        backButton.onClick.RemoveListener(OnBackButtonClicked);
    }

    private void OnBackButtonClicked()
    {
        SceneManager.LoadScene("CategoryScene");
    }

    private void OnShowHint()
    {
        hintText.gameObject.SetActive(true);
        if (audioSource != null)
            audioSource.Play();
    }

    public void SetWord(Word word)
    {
        if (word == null)
        {
            Debug.LogError("Null word passed to SetWord");
            return;
        }

        wordText.text = word.word;
        hintText.text = word.hint;
    }

    public void HideHint()
    {
        hintText.gameObject.SetActive(false);
    }
}
