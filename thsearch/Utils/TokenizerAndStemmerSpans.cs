namespace thsearch;
using CommunityToolkit.HighPerformance.Enumerables;
using CommunityToolkit.HighPerformance;
using System.Diagnostics;
using System.Linq;


class TokenizerSpans: ITokenizer
{

    private readonly char[] specialChars;

    private readonly string[] suffixes;

    private readonly HashSet<string> stopWords;



    public TokenizerSpans(char[] trimChars = null, string[] suffixes = null, HashSet<string> stopWords = null)
    {

        this.specialChars = trimChars ??
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



    public List<string> Process(string text)
    {


        // TODO: 
        // 1. Remove special characters before spliting.
        // 2. Split
        // 3. Remove words 2 characters or less 
        // 4. stop word removal and empty string check


        // Examine token for stop words, suffixes, then punctuation chars--create a new span each time

        // Then add to string List stems

        List<string> stems = new List<string>();

        foreach (ReadOnlySpan<char> token in text.Tokenize(' '))
        {

            ReadOnlySpan<char> trimmedToken = RemoveSpecialChars(token);

            // Remove any suffixes from the token
            ReadOnlySpan<char> stemmedSpan = RemoveSuffixes(trimmedToken);

            string stemmedString = stemmedSpan.ToString().ToLower();

            if (stopWords.Contains(stemmedString) || string.IsNullOrEmpty(stemmedString))
            {
                continue;
            }



            stems.Add(stemmedString);
        }

        return stems;
    }

    private ReadOnlySpan<char> RemoveSuffixes(ReadOnlySpan<char> token)
    {
        foreach (string suffix in suffixes)
        {
            if (token.EndsWith(suffix, StringComparison.Ordinal))
            {
                return token.Slice(0, token.Length - suffix.Length);
            }
        }
        return token;
    }

    private ReadOnlySpan<char> RemoveSpecialChars(ReadOnlySpan<char> token)
    {
        Span<char> outputSpan = new char[token.Length];

        int outputIndex = 0;
        foreach (char c in token)
        {
            if (char.IsLetter(c))
            {
                outputSpan[outputIndex++] = c;
            }
        }

        ReadOnlySpan<char> outPut = outputSpan.Slice(0, outputIndex);

        return outPut;
    }

}