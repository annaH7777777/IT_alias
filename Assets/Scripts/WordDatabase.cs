using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[CreateAssetMenu(fileName = "WordDatabase", menuName = "AliasGame/WordDatabase")]
public class WordDatabase : ScriptableObject
{
    [System.Serializable]
    public class WordEntry
    {
        public string word;
        public string level;
        public string hint;
    }

    public List<WordEntry> wordList = new List<WordEntry>();

    public void AddEntry(string word, string level, string hint)
    {
        wordList.Add(new WordEntry { word = word, level = level, hint = hint });
    }
    
    public void SaveDatabase()
    {
#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
        Debug.Log("Word Database Saved!");
#endif
    }
}