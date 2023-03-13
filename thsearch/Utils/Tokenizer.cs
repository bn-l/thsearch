namespace thsearch;


class Tokenizer
{
    private readonly char[] punctuationChars;

    private readonly string[] suffixes;

    private readonly string[] stopWords;


    public Tokenizer(char[] trimChars = null, string[] suffixes = null)
    {

        this.punctuationChars = trimChars ??
            Enumerable
                .Range(char.MinValue, char.MaxValue - char.MinValue + 1)
                .Where(c => !char.IsLetter((char)c))
                .Select(c => (char)c)
                .ToArray();

        this.suffixes = suffixes ?? new string[] 
            { 
                "able", "al", "ed", "en", "er", "ful", "ic", "ing", "ion", "less", "ly", "ment", "ness", "ous", "s", "tion", "ty", "y", "'s"
            };

        this.stopWords = stopWords ?? new String[]
            {
                "a", "an",  "and",  "are",  "as",  "at",  "be",  "but",  "by",  "for",  "if",  "in",  "into",  "is",  "it", "not",  "of",  "on",  "or",  "such",  "that",  "the",  "their",  "then",  "there",  "these",  "they",  "this",  "to",  "was",  "will",  "with"
            };

    }

    // !! TODO: remove special characters including white space

    public List<string> Process(string text)
    {
        // a new string is being created each time

        string[] rawTokens = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        //converts all tokens to lowercase
        string[] lowercased = rawTokens.Select(token => token.ToLower()).ToArray();

        string[] stopWordless = lowercased.Except(stopWords).ToArray();

        string[] punctuationless = stopWordless.Select(token => token.Trim(this.punctuationChars)).ToArray();

        string[] suffixless = punctuationless.Select(token => this.suffixes.Aggregate(token, (current, suffix) => current.EndsWith(suffix) ? current.Substring(0, current.Length - suffix.Length) : current)).ToArray();

        return suffixless.ToList();

    }

}