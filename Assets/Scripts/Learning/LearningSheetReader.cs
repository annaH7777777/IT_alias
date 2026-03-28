using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class LearningSheetReader : MonoBehaviour
{
    [TextArea]
    public string googleSheetURL = "https://docs.google.com/spreadsheets/d/e/your_id/pub?output=csv";

    public List<LearningWord> words = new List<LearningWord>();

    private static LearningSheetReader _instance;

    public static LearningSheetReader Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("[LearningSheetReader]");
                _instance = go.AddComponent<LearningSheetReader>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Load(string url, Action<bool> callback)
    {
        googleSheetURL = url;
        StartCoroutine(FetchCSV(callback));
    }

    private IEnumerator FetchCSV(Action<bool> callback)
    {
        var request = UnityWebRequest.Get(googleSheetURL);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to load learning sheet: " + request.error);
            callback?.Invoke(false);
            yield break;
        }

        string text = request.downloadHandler.text;

        // Detect if Google returned an HTML page instead of CSV
        if (text.TrimStart().StartsWith("<") || text.Contains("<!DOCTYPE") || text.Contains("<html"))
        {
            Debug.LogError("Google Sheet returned HTML instead of CSV. Make sure the URL uses 'pub?output=csv' format. " +
                "Go to File > Share > Publish to web > select CSV format.");
            callback?.Invoke(false);
            yield break;
        }

        ParseCSV(text);
        callback?.Invoke(true);
    }

    private void ParseCSV(string csvText)
    {
        words.Clear();
        string[] lines = csvText.Split('\n');

        // Skip header row (Word, Translation, Synonym, Pronunciation, Date)
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            var values = CSVParser.SplitLine(line);
            if (values.Count < 2)
            {
                Debug.LogWarning($"Skipping malformed learning CSV line {i}: {line}");
                continue;
            }

            string word = values[0];
            string translation = values.Count > 1 ? values[1] : "";
            string synonym = values.Count > 2 ? values[2] : "";
            string pronunciation = values.Count > 3 ? values[3] : "";
            string date = values.Count > 4 ? values[4] : "";

            if (string.IsNullOrEmpty(word)) continue;

            words.Add(new LearningWord(word, translation, synonym, pronunciation, date));
        }

        Debug.Log($"Loaded {words.Count} learning words.");
    }
}
