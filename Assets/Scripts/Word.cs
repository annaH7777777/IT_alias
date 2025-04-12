using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Word
{
    public string word;
    public string level;
    public string hint;

    public Word(string word, string level, string hint)
    {
        this.level = level;
        this.word = word;
        this.hint = hint;
    }
}

