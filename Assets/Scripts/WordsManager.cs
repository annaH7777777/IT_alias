using UnityEngine;

public class WordsManager : MonoBehaviour
{
    [SerializeField] WordUiController wordUi;

    void Start()
    {
        string currentLevel = PlayerPrefs.GetString("Level", "A1");
        Debug.Log("currentLevel " + currentLevel);

        CSVReader csvReader = FindObjectOfType<CSVReader>();
        if (csvReader == null)
        {
            Debug.LogError("CSVReader not found in scene");
            return;
        }

        Word word = csvReader.GetRandomWord(currentLevel);
        if (word == null)
        {
            Debug.LogError($"No words available for level {currentLevel}");
            return;
        }

        Debug.Log(word.word + " " + word.hint + " " + word.level);
        wordUi.SetWord(word);
    }
}
