namespace BibleTextScraper;

struct Word
{
    public string Verse { get; set; }
    public string GreekWord { get; set; }
    public string EnglishTranslation { get; set; }

    public Word(string _verse, string _greekWord, string _englishTranslation)
    {
        Verse = _verse
            .Split(" Greek ")[0]
            .Replace(',', ' ')
            .Replace('.', ' ')
            .Trim()
            .ToLower();

        GreekWord = _greekWord
            .Split("de")[0]
            .Replace(',', ' ')
            .Replace('.', ' ')
            .Replace(':', ' ')
            .Trim()
            .ToLower();

        EnglishTranslation = _englishTranslation
            .Replace(',', ' ')
            .Replace('.', ' ')
            .Replace(':', ' ')
            .Trim()
            .ToLower();
    }
}