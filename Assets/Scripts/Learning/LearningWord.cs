[System.Serializable]
public class LearningWord
{
    public string word;
    public string translation;
    public string synonym;
    public string pronunciation;
    public string date;

    public LearningWord(string word, string translation, string synonym, string pronunciation, string date)
    {
        this.word = word;
        this.translation = translation;
        this.synonym = synonym;
        this.pronunciation = pronunciation;
        this.date = date;
    }
}
