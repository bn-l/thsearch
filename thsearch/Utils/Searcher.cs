namespace thsearch;
    

using System.Globalization;
using System.Diagnostics;

// Searcher is a utility class that provides methods that match the delegate type parameter of Index.Search. It is initialized with a Tokenizer and a Stemmer.

// TODO: InverseIndex is a concurrent dictionary, can it be multithreaded searched? (Could have one producer thread that iterates over the keys of the dictionary, adding each to a concurrentqueue)


class Searcher {

    private Tokenizer tokenizer;

    public Searcher(Tokenizer tokenizer) {

        this.tokenizer = tokenizer;
        
    }

    // Enumerates over inverseIndex looking for query. It will then rank the results using the Tf-Idf method and return an array of int pathIds
    public int[] TfIdf(Index index, string query)
    {

        if(string.IsNullOrEmpty(query)) throw new ArgumentException("Query cannot be null or empty");

       
        IEnumerable<string> queryTokens = tokenizer.Process(query).Distinct();
        
        // path, score (determines result ranks)  pathId: resultScore
        Dictionary<int, double> resultScores = new Dictionary<int, double>();
        
        // Update the score for each document path in scores dict by iterating over each token
        
        foreach (var queryToken in queryTokens)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            // try get value and out it. If it's not there, skip this iteration (continue)

            if (!index.TryLookUpToken(queryToken, out var ranksDictionary)) continue;

            stopwatch.Stop();
            Console.WriteLine($"It took {stopwatch.ElapsedMilliseconds} ms to lookup {queryToken}");

            int totalDocs = index.GetFileCount();
            int matchingDocs = ranksDictionary.Count;
            // idf will be bigger, and give more weight to, terms that are relatively rare in the corpus
            double idf = Math.Log10((double) (totalDocs + 1 / matchingDocs));

            // Go over each document in the ranksDict and get frequency of the term in the documents
            foreach (var (document, termFreqs) in ranksDictionary)
            {
                double tf = (double) termFreqs.Count;
                double tfIdf = tf * idf;

                
                // Adds tfIdf to current score of document in resultScores
                resultScores[document] = resultScores.GetValueOrDefault(document, 0) + tfIdf;
            
            }

        }

       return resultScores
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