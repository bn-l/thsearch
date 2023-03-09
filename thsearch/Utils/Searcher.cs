namespace thsearch;
    
using System.Collections.Concurrent;
using System.Globalization;

// Searcher is a utility class that provides methods that match the delegate type parameter of Index.Search. It is initialized with a Tokenizer and a Stemmer.



class Searcher {

    private TokenizerAndStemmer tokenizerAndStemmer;

    public Searcher(TokenizerAndStemmer tokenizerAndStemmer) {

        this.tokenizerAndStemmer = tokenizerAndStemmer;
        
    }

    // Enumerates over inverseIndex looking for query. It will then rank the results using the Tf-Idf method and return an array of string paths
    public string[] TfIdf(ConcurrentDictionary<string, FileIndexEntry> fileIndex, ConcurrentDictionary<string, InverseIndexEntry> inverseIndex, string query)
    {


        IEnumerable<string> queryTokens = tokenizerAndStemmer.Process(query).Distinct();
        

        Dictionary<string, double> rankScores = new Dictionary<string, double>();
        
        // Update the score for each document path in scores dict by iterating over each token
        
        foreach (var queryToken in queryTokens)
        {

            // Get the first key in the inverse dictionary the matches (according to CustomContains) the queryToken

            string key = inverseIndex.Keys.Where(k => CustomContains(k, queryToken)).FirstOrDefault();
            

            if (!inverseIndex.TryGetValue(key, out InverseIndexEntry inverseIndexEntry))
                continue;

            int totalDocs = fileIndex.Count;
            int matchingDocs = inverseIndexEntry.RanksDict.Count;
            // idf will be bigger, and give more weight to, terms that are relatively rare in the corpus
            double idf = Math.Log10((double) totalDocs / matchingDocs);

            // Go over each document in the ranksDict and get frequency of the term in the documents
            foreach (var (document, termFreqs) in inverseIndexEntry.RanksDict)
            {
                double tf = (double) termFreqs.Count;
                double tfIdf = tf * idf;

                rankScores.TryGetValue(document, out double currentScore);
                rankScores[document] = currentScore + tfIdf;

            }

        }

       return rankScores
            .OrderByDescending(pair => pair.Value)
            .Select(pair => pair.Key)
            .ToArray();

    }

    // E.g. Matches: übEr and uber
    internal static bool CustomContains(string source, string toCheck)
    {
        CompareInfo ci = new CultureInfo("en-US").CompareInfo;
        CompareOptions co = CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace;
        return ci.IndexOf(source, toCheck, co) != -1;
    }
}


// var InverseIndex = {
//     "dogs":  {
//         "/some/path/dog1.txt": [1, 2, 5, 77, 345],
//         "/some/path/dog2.txt": [6, 7, 22, 217],
//     },
//     "cats":  {
//         "/some/path/cat1.txt": [8, 9, 10],
//         "/some/path/cat2.txt": [13, 14, 16],
//         "/some/path/cat5.txt": [31, 39, 42, 48, 52],
//     }
// }