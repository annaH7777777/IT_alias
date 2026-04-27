using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        if (lines.Length == 0) return;

        var header = CSVParser.SplitLine(lines[0].Trim());
        int wordIdx = FindColumn(header, "word");
        int translationIdx = FindColumn(header, "translation/meaning", "translation", "meaning");
        int synonymIdx = FindColumn(header, "synonym");
        int pronunciationIdx = FindColumn(header, "pronunciation");
        int dateIdx = FindColumn(header, "date");
        int categoryIdx = FindColumn(header, "category");
        int orderIdx = FindColumn(header, "order");

        if (wordIdx < 0)
        {
            Debug.LogError("CSV is missing a 'Word' column in the header row.");
            return;
        }

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            var values = CSVParser.SplitLine(line);
            string word = GetField(values, wordIdx);
            if (string.IsNullOrEmpty(word)) continue;

            string translation = GetField(values, translationIdx);
            string synonym = GetField(values, synonymIdx);
            string pronunciation = GetField(values, pronunciationIdx);
            string date = GetField(values, dateIdx);
            string category = GetField(values, categoryIdx);
            int order = 0;
            if (orderIdx >= 0 && orderIdx < values.Count)
                int.TryParse(values[orderIdx], out order);

            words.Add(new LearningWord(word, translation, synonym, pronunciation, date, category, order));
        }

        Debug.Log($"Loaded {words.Count} learning words.");
    }

    private static int FindColumn(List<string> header, params string[] candidates)
    {
        for (int i = 0; i < header.Count; i++)
        {
            string h = header[i].Trim().ToLowerInvariant();
            foreach (var c in candidates)
                if (h == c) return i;
        }
        return -1;
    }

    private static string GetField(List<string> values, int idx)
    {
        if (idx < 0 || idx >= values.Count) return "";
        return values[idx];
    }

    public List<string> GetUniqueCategories()
    {
        return words
            .Select(w => w.category)
            .Where(c => !string.IsNullOrEmpty(c))
            .Distinct()
            .OrderBy(c => c)
            .ToList();
    }

    private static readonly string[] MonthAbbrs = {
        "Jan", "Feb", "Mar", "Apr", "May", "Jun",
        "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"
    };

    public static bool TryParseDate(string dateStr, out int year, out int month, out int day)
    {
        year = month = day = 0;
        if (string.IsNullOrEmpty(dateStr)) return false;
        var parts = dateStr.Split('-');
        if (parts.Length != 3) return false;
        if (!int.TryParse(parts[0], out day)) return false;
        month = Array.FindIndex(MonthAbbrs, m => string.Equals(m, parts[1], StringComparison.OrdinalIgnoreCase)) + 1;
        if (month == 0) return false;
        if (!int.TryParse(parts[2], out year)) return false;
        return true;
    }

    public static string MonthName(int month) => MonthAbbrs[month - 1];

    public List<int> GetUniqueYears()
    {
        var years = new HashSet<int>();
        foreach (var w in words)
            if (TryParseDate(w.date, out int y, out _, out _))
                years.Add(y);
        return years.OrderByDescending(y => y).ToList();
    }

    public List<int> GetUniqueMonths(int? yearFilter)
    {
        var months = new HashSet<int>();
        foreach (var w in words)
        {
            if (!TryParseDate(w.date, out int y, out int m, out _)) continue;
            if (yearFilter.HasValue && y != yearFilter.Value) continue;
            months.Add(m);
        }
        return months.OrderBy(m => m).ToList();
    }

    public List<int> GetUniqueDays(int? yearFilter, int? monthFilter)
    {
        var days = new HashSet<int>();
        foreach (var w in words)
        {
            if (!TryParseDate(w.date, out int y, out int m, out int d)) continue;
            if (yearFilter.HasValue && y != yearFilter.Value) continue;
            if (monthFilter.HasValue && m != monthFilter.Value) continue;
            days.Add(d);
        }
        return days.OrderBy(d => d).ToList();
    }

    public List<LearningWord> GetFilteredWords(string category, int? year, int? month, int? day)
    {
        return words.Where(w =>
        {
            if (!string.IsNullOrEmpty(category) && w.category != category) return false;
            if (!year.HasValue && !month.HasValue && !day.HasValue) return true;
            if (!TryParseDate(w.date, out int y, out int m, out int d)) return false;
            if (year.HasValue && y != year.Value) return false;
            if (month.HasValue && m != month.Value) return false;
            if (day.HasValue && d != day.Value) return false;
            return true;
        }).ToList();
    }
}
