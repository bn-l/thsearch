namespace thsearch;


class Tokenizer
{
    private readonly char[] punctuationChars;

    private readonly string[] suffixes;

    private readonly HashSet<string> stopWords;


    public Tokenizer(char[] trimChars = null, string[] suffixes = null, HashSet<string> stopWords = null)
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
        this.suffixes = this.suffixes.OrderByDescending(x => x.Length).ToArray();

        this.stopWords = stopWords ?? new HashSet<string>
            {
                "a", "an",  "and",  "are",  "as",  "at",  "be",  "but",  "by",  "for",  "if",  "in",  "into",  "is",  "it", "not",  "of",  "on",  "or",  "such",  "that",  "the",  "their",  "then",  "there",  "these",  "they",  "this",  "to",  "was",  "will",  "with"
            };

    }

    // TODO: remove special characters, escapes sequences, \n, \t, etc.

    public string[] Process(string text)
    {
        // lower case split of words
        string[] tokens = text.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);

        List<string> processedTokens = new List<string>();

        foreach (string token in tokens)
        {
            // skip stop words
            if (stopWords.Contains(token))
            {
                continue;
            }

            // trim punctuation 
            string processedToken = token.Trim(this.punctuationChars);

            //can also be a contains call with suffixes a hashset
            foreach (string suffix in this.suffixes)
            {
                if (processedToken.EndsWith(suffix))
                {
                    processedToken = processedToken.Substring(0, processedToken.Length - suffix.Length);
                    break;
                }
            }

            processedTokens.Add(processedToken);
        }

        return processedTokens.ToArray();

    }

}