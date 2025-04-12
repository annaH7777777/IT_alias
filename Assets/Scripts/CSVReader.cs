using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;

public class CSVReader : MonoBehaviour
{
    public List<Word> a1WordList = new List<Word>();
    public List<Word> a2WordList = new List<Word>();
    public List<Word> b1WordList = new List<Word>();
    public List<Word> b2WordList = new List<Word>();
    public List<Word> c1WordList = new List<Word>();
    public List<Word> c2WordList = new List<Word>();

    void Awake()
    {
        StartCoroutine(LoadCSV());
        DontDestroyOnLoad(this.gameObject);
    }

    IEnumerator LoadCSV()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "words.csv");
        
        UnityWebRequest request = UnityWebRequest.Get(path);
        yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                ProcessCSV(request.downloadHandler.text);
            }
            else
            {
                Debug.LogError("Failed to load CSV: " + request.error);
            }
    }

    void ProcessCSV(string csvText)
    {
            string[] lines = csvText.Split('\n');

            // Start from index 1 to skip the header row
            for (int i = 1; i < lines.Length; i++)
            {
                string[] values = lines[i].Split(new char[] { ',' }, 3); // Split only at the first comma

                if (values.Length == 3)
                {
                    string word = values[0].Trim();
                    string level = values[1].Trim();
                    string text = values[2].Trim();
                    Word entry = new Word(word, level, text);
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
            int wordCount = a1WordList.Count + a2WordList.Count + b1WordList.Count + c1WordList.Count + c2WordList.Count + b2WordList.Count;
            Debug.Log($"Loaded {wordCount} words from CSV.");
    }

    public Word GetRandomWord(string level)
    {
        switch (level)
        {
            case "A1":
                return a1WordList[Random.Range(0, a1WordList.Count)];
            case "A2":
                return a2WordList[Random.Range(0, a2WordList.Count)];
            case "B1":
                return b1WordList[Random.Range(0, b1WordList.Count)];
            case "B2":
                return b2WordList[Random.Range(0, b2WordList.Count)];
            case "C1":
                return c1WordList[Random.Range(0, c1WordList.Count)];
            case "C2":
                return c2WordList[Random.Range(0, c2WordList.Count)];
            default:
                return null;
        }
    }
}