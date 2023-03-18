namespace thsearch;
using CommunityToolkit.HighPerformance.Enumerables;
using CommunityToolkit.HighPerformance;
using System.Diagnostics;
using System.Linq;

// TODO: Fix this and test out its perfomance vs the string.split tokenizer

class TokenizerSpans: ITokenizer
{

    private readonly char[] punctuationChars;

    private readonly string[] suffixes;

    private readonly HashSet<string> stopWords;



    public TokenizerSpans(char[] trimChars = null, string[] suffixes = null, HashSet<string> stopWords = null)
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



    public List<string> Process(string text)
    {

        // TODO: Special characters are converted to escape sequences. E.g. ë becomes \u00EB

        // TODO: Stop word removal

        // Examine token for stop words, suffixes, then punctuation chars--create a new span each time

        // Then add to string List stems

        List<string> stems = new List<string>();

        foreach (ReadOnlySpan<char> token in text.Tokenize(' '))
        {
            // Remove any leading or trailing punctuation characters from the token
            // ReadOnlySpan<char> trimmedToken = token.Trim(punctuationChars);

            ReadOnlySpan<char> trimmedToken = RemovePunctuation(token);

            // Remove any suffixes from the token
            ReadOnlySpan<char> stemmedSpan = RemoveSuffixes(trimmedToken);

            string stemmedString = stemmedSpan.ToString().ToLower();

            // Check if the resulting token is a stop word
            if (stopWords.Contains(stemmedString))
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

    private ReadOnlySpan<char> RemovePunctuation(ReadOnlySpan<char> token)
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