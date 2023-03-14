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

    // TODO: remove special characters, escapes sequences, \n, \t, etc.

    public string[] Process(string text)
    {
        // lower case split of words
        string[] tokens = text.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);

        // take out stop word
        tokens = tokens.Except(stopWords).ToArray();

        for (int i = 0; i < tokens.Length; i++)
        {
            
            // trim punctuation
            tokens[i] = tokens[i].Trim(this.punctuationChars);

            // remove suffix
            foreach (string suffix in this.suffixes)
            {
                if (tokens[i].EndsWith(suffix))
                {
                    tokens[i] = tokens[i].Substring(0, tokens[i].Length - suffix.Length);
                    break;
                }
            }
        }

        return tokens;

    }

}