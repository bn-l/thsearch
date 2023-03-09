namespace thsearch;


class FileIndexEntry
{
    public DateTime LastModified { get; set; }
    public List<string> Stems { get; set; }

    public FileIndexEntry(DateTime lastModified, List<string> stems)
    {
        this.LastModified = lastModified;
        this.Stems = stems;
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