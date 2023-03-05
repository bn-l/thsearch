namespace thsearch;

// needs to be a dictionary

class InverseIndexEntry
{
    public Dictionary<string, List<int>> Ranks { get; set; }

    public InverseIndexEntry(string path, int index)
    {
        Ranks = new Dictionary<string, List<int>>()
        {
            { path, new List<int>() { index } }
        };
    }
}



/*

{
    "dogs": StemDict {
        "/some/path/dog1.txt": [1, 2, 5, 77, 345],
        "/some/path/dog2.txt": [6, 7, 22, 217],
    },
    "cats": {
        "/some/path/cat1.txt": [8, 9, 10],
        "/some/path/cat2.txt": [13, 14, 16],
        "/some/path/cat5.txt": [31, 39, 42, 48, 52],
    }
}

*/