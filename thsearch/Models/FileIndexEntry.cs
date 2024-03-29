namespace thsearch;


class FileIndexEntry
{
    public DateTime LastModified { get; }
    public List<string> Stems { get; }
    public HashSet<string> StemSet { get; }
    public Dictionary<string, int> stemFrequency;

    public FileIndexEntry(DateTime lastModified, List<string> stems)
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