namespace thsearch;


class FileIndexEntry
{
    public DateTime LastModified { get; }
    public string[] Stems { get; }
    public HashSet<string> StemSet { get; }
    public Dictionary<string, int> stemFrequency;

    // TODO: Implement IDisposable so this can be disposed of in a using closure after being saved to index

    public FileIndexEntry(DateTime lastModified, string[] stems)
    {
        this.LastModified = lastModified;
        this.Stems = stems;
        this.stemFrequency = new Dictionary<string, int>();
        this.StemSet = new HashSet<string>(stems);

        foreach (string stem in stems)
        {
            if (stemFrequency.ContainsKey(stem))
            {
                stemFrequency[stem]++;
            }
            else
            {
                stemFrequency.Add(stem, 1);
            }
        }
    }

    

}


/*

{
    "some/path/fileName.txt":
        {
            "lastModified": "2022-07-14T01:00:00+01:00",
            "stems": ["dog", "fat", "grey", "nice"]
        },
    "some/path2/fileName2.txt":
        {
            "lastModified": "2022-05-12T01:00:00+01:00",
            "stems": ["cat", "slim", "red", "mean"]
        },
}

*/