namespace thsearch;
using CommunityToolkit.HighPerformance.Enumerables;
using CommunityToolkit.HighPerformance;
using System.Diagnostics;

// TODO: Fix this and test out its perfomance vs the string.split tokenizer

class TokenizerAndStemmer
{

    private readonly char[] punctuationChars;

    private readonly string[] suffixes;

    private readonly HashSet<string> stopWords;


    public TokenizerAndStemmer(char[] trimChars = null, string[] suffixes = null, HashSet<string> stopWords = null)
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
       

        List<string> stems = new List<string>();

        // Each ReadOnlySpan<char> token in this loop is trimmed of ',' and added to the return value
        foreach (ReadOnlySpan<char> token in text.Tokenize(' '))
        {
            // TODO: Special characters are converted to escape sequences. E.g. ë becomes \u00EB

            // TODO: Stop word removal

            // Examine token for stop words, suffixes, then punctuation chars--create a new span each time

            // Then add to string List stems


            


            ReadOnlySpan<char> tokenPunctuationTrimmed = token.Trim(this.punctuationChars.AsSpan());

            foreach (string suffix in this.suffixes)
            {
                if (tokenPunctuationTrimmed.EndsWith(suffix))
                {
                    int suffixLength = suffix.Length;
                    ReadOnlySpan<char> stem =
                        tokenPunctuationTrimmed.Slice
                        (
                            0, tokenPunctuationTrimmed.Length - suffixLength
                        );

                    // Punctuation removed, suffixes removed, token is now a "stem". Convert to a string.
                    stems.Add(new string(stem));

                }
            }
        }

        return stems;
    }

}