using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChallengeController : MonoBehaviour
{
    [SerializeField] WordUiController wordUi;
    [SerializeField] Button nextButton;
    [SerializeField] CSVReader csvReader;
    
    void Start()
    {
        //string currentLevel = PlayerPrefs.GetString("Level", "A1");
        //Debug.Log("currentLevel " + currentLevel);
        nextButton.onClick.AddListener(OnNextClick);
        
        csvReader = FindObjectOfType<CSVReader>();
        SetWord();
    }

    private void OnNextClick()
    {
       SetWord();
    }

    private void SetWord()
    {
        Word word = csvReader.GetNextWord();
        Debug.Log(word.word + " " + word.hint + " " + word.level);
        wordUi.SetWord(word);
        wordUi.HideHint();
    }
}
