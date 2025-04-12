using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WordsManager : MonoBehaviour
{
    [SerializeField] WordUiController wordUi;
    [SerializeField] CSVReader csvReader;
    
    void Start()
    {
        string currentLevel = PlayerPrefs.GetString("Level", "A1");
        Debug.Log("currentLevel " + currentLevel);
        
        csvReader = FindObjectOfType<CSVReader>();
        Word word = csvReader.GetRandomWord(currentLevel);
        Debug.Log(word.word + " " + word.hint + " " + word.level);
        wordUi.SetWord(word);
    }
}
