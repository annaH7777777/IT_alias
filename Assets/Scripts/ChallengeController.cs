using UnityEngine;
using UnityEngine.UI;

public class ChallengeController : MonoBehaviour
{
    [SerializeField] WordUiController wordUi;
    [SerializeField] Button nextButton;

    private CSVReader csvReader;

    void Start()
    {
        nextButton.onClick.AddListener(OnNextClick);

        csvReader = FindObjectOfType<CSVReader>();
        if (csvReader == null)
        {
            Debug.LogError("CSVReader not found in scene");
            return;
        }

        SetWord();
    }

    private void OnDestroy()
    {
        nextButton.onClick.RemoveListener(OnNextClick);
    }

    private void OnNextClick()
    {
        SetWord();
    }

    private void SetWord()
    {
        Word word = csvReader.GetNextWord();
        if (word == null)
        {
            Debug.LogError("No words available");
            return;
        }

        Debug.Log(word.word + " " + word.hint + " " + word.level);
        wordUi.SetWord(word);
        wordUi.HideHint();
    }
}
