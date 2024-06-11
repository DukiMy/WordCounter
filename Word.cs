namespace BibleTextScraper;

struct Word
{
    public string Verse { get; set; }
    public string GreekWord { get; set; }
    public string EnglishTranslation { get; set; }

    public Word(string _verse, string _greekWord, string _englishTranslation)
    {
        Verse = _verse
            .Trim()
            .Split(" Greek ")[0]
            .Replace(',', ' ')
            .Replace('.', ' ')
            .ToLower();

        GreekWord = _greekWord
            .Trim()
            .Split("de")[0]
            .Replace(',', ' ')
            .Replace('.', ' ')
            .Replace(':', ' ')
            .ToLower();

        EnglishTranslation = _englishTranslation
            .Trim()
            .Replace(',', ' ')
            .Replace('.', ' ')
            .Replace(':', ' ')
            .ToLower();
    }
}