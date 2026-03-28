using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using Random = UnityEngine.Random;

public class CSVReader : MonoBehaviour
{
    [TextArea]
    public string googleSheetURL = "https://docs.google.com/spreadsheets/d/e/your_id/pub?output=csv";
    [SerializeField] bool _readFromGoogleSheet = false;
    private int _fullListIndex = 0;
    
    public List<Word> a1WordList = new List<Word>();
    public List<Word> a2WordList = new List<Word>();
    public List<Word> b1WordList = new List<Word>();
    public List<Word> b2WordList = new List<Word>();
    public List<Word> c1WordList = new List<Word>();
    public List<Word> c2WordList = new List<Word>();
    public List<Word> fullList = new List<Word>();
    

    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    public void StartLoading(bool readFromGoogleSheet, Action<bool> callback)
    {
        StartCoroutine(LoadCSV(readFromGoogleSheet, callback));
    }

    IEnumerator LoadCSV(bool readFromGoogleSheet, Action<bool> callback)
    {
        string path = readFromGoogleSheet ? googleSheetURL : Path.Combine(Application.streamingAssetsPath, "words.csv");
        Debug.Log("Path is " + path);
        
        UnityWebRequest request = UnityWebRequest.Get(path);
        yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                ProcessCSV(request.downloadHandler.text, callback);
            }
            else
            {
                Debug.LogError("Failed to load CSV: " + request.error);
                callback?.Invoke(false);
            }
    }

    void ProcessCSV(string csvText, Action<bool> callback)
    {
            string[] lines = csvText.Split('\n');

            // Start from index 1 to skip the header row
            for (int i = 1; i < lines.Length; i++)
            {
                string[] values = lines[i].Split(new char[] { ',' }, 3); // Split only at the first comma

                if (values.Length < 3)
                {
                    Debug.LogWarning($"Skipping malformed CSV line {i}: {lines[i]}");
                    continue;
                }

                {
                    string word = values[0].Trim();
                    string level = values[1].Trim();
                    string text = values[2].Trim();

                    if (string.IsNullOrEmpty(word) || string.IsNullOrEmpty(level) || string.IsNullOrEmpty(text))
                    {
                        Debug.LogWarning($"Skipping CSV line {i} with empty fields");
                        continue;
                    }

                    Word entry = new Word(word, level, text);

                    fullList.Add(entry);
                    
                    switch (level)
                    {
                        case "A1":
                            a1WordList.Add(entry);
                            break;
                        case "A2":
                            a2WordList.Add(entry);
                            break;
                        case "B1":
                            b1WordList.Add(entry);
                            break;
                        case "B2":
                            b2WordList.Add(entry);
                            break;
                        case "C1":
                            c1WordList.Add(entry);
                            break;
                        case "C2":
                            c2WordList.Add(entry);
                            break;
                        default:
                            Debug.Log($"Unknown level: {level}");
                            break;
                    }
                }
            }
            Shuffle(fullList);
            
            int wordCount = a1WordList.Count + a2WordList.Count + b1WordList.Count + c1WordList.Count + c2WordList.Count + b2WordList.Count;
            Debug.Log($"Loaded {wordCount} words from CSV.");
            callback?.Invoke(true);
    }
    
    void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randIndex = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[randIndex];
            list[randIndex] = temp;
        }
    }
    
    public Word GetNextWord()
    {
        if (fullList.Count == 0)
        {
            Debug.LogWarning("⚠ fullList is empty.");
            return null;
        }

        Word nextWord = fullList[_fullListIndex];
        _fullListIndex = (_fullListIndex + 1) % fullList.Count; // wrap around
        return nextWord;
    }


    public Word GetRandomWord(string level)
    {
        List<Word> list = level switch
        {
            "A1" => a1WordList,
            "A2" => a2WordList,
            "B1" => b1WordList,
            "B2" => b2WordList,
            "C1" => c1WordList,
            "C2" => c2WordList,
            _ => null
        };

        if (list == null || list.Count == 0)
        {
            Debug.LogWarning($"No words available for level: {level}");
            return null;
        }

        return list[Random.Range(0, list.Count)];
    }
}