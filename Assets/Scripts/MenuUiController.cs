using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuUiController : MonoBehaviour
{
    [SerializeField] Button startButton;
    [SerializeField] Button learnButton;
    [SerializeField] Toggle sourceToggle;
    [SerializeField] CSVReader csvReader;
    [SerializeField] TextMeshProUGUI errorText;

    [Header("Learning Words")]
    [TextArea]
    [SerializeField] string learningSheetURL = "https://docs.google.com/spreadsheets/d/e/your_id/pub?output=csv";

    void Start()
    {
        startButton.onClick.AddListener(OnStartClicked);

        if (learnButton != null)
            learnButton.onClick.AddListener(OnLearnClicked);
    }

    private void OnDestroy()
    {
        startButton.onClick.RemoveAllListeners();
        if (learnButton != null)
            learnButton.onClick.RemoveAllListeners();
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

    private void OnLearnClicked()
    {
        errorText.text = "Loading words...";

        LearningSheetReader.Instance.Load(learningSheetURL, success =>
        {
            if (success)
            {
                SceneManager.LoadScene("FlashcardScene");
            }
            else
            {
                errorText.text = "Failed to load learning words.";
            }
        });
    }
}
