namespace thsearch;
using CommunityToolkit.HighPerformance.Enumerables;
using CommunityToolkit.HighPerformance;


class TokenizerAndStemmer
{

    private readonly char[] punctuationChars;

    private readonly string[] suffixes;


    public TokenizerAndStemmer(char[] trimChars = null, string[] suffixes = null)
    {

        this.punctuationChars = trimChars ??
            Enumerable
                .Range(char.MinValue, char.MaxValue - char.MinValue + 1)
                .Where(c => !char.IsLetter((char)c))
                .Select(c => (char)c)
                .ToArray();

        this.suffixes = suffixes ?? new string[] 
            { 
                "able", "al", "ed", "en", "er", "est", "ful", "ic", "ing", "ion", "ish", "ive", "less", "ly", "ment", "ness", "ous", "s", "tion", "ty", "y", "'s"
            };

    }


    public List<string> Process(string text)
    {

        List<string> stems = new List<string>();

        // Each ReadOnlySpan<char> token in this loop is trimmed of ',' and added to the return value
        foreach (ReadOnlySpan<char> token in text.Tokenize(' '))
        {
            // !! TODO: Special characters are converted to escape sequences. E.g. � becomes \u00EB

            // !! TODO: Stop word removal

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
                    stems.Add(stem.ToString());

                }
            }
        }

        return stems;
    }

}