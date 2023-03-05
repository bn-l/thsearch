namespace thsearch;

// An InverseIndexEntry is an an array of InverseIndexEntryItems
//

class InverseIndexValue
{
    public string Path { get; set; }
    public List<int> Ranks { get; set; }
}


/*

    {
    "dog": 
        path: "/some/path/dog1.txt"
        ranks: "[1, 2, 5, 77, 345],
    }
*/