namespace thsearch;


class FileIndexEntry
{
   public Dictionary<string, FileIndexValue> Entry { get; set; }

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