[System.Serializable]
public class LearningWord
{
    public string word;
    public string translation;
    public string synonym;
    public string pronunciation;
    public string date;
    public string category;
    public int order;

    public LearningWord(string word, string translation, string synonym, string pronunciation, string date, string category, int order = 0)
    {
        this.word = word;
        this.translation = translation;
        this.synonym = synonym;
        this.pronunciation = pronunciation;
        this.date = date;
        this.category = category;
        this.order = order;
    }
}
