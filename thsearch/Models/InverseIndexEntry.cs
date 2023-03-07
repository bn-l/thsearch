namespace thsearch;

using System.Collections.Concurrent;

// needs to be a dictionary

class InverseIndexEntry
{
    public ConcurrentDictionary<string, List<int>> RanksDict { get; set; }


    public InverseIndexEntry(string path, List<int> index)
    {
        RanksDict = new ConcurrentDictionary<string, List<int>>()
        {
            [path] = index //object initializer. see: 1
        };

    }
}

//1: https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/object-and-collection-initializers


/*

{
    "dogs":  {
        "/some/path/dog1.txt": [1, 2, 5, 77, 345],
        "/some/path/dog2.txt": [6, 7, 22, 217],
    },
    "cats":  {
        "/some/path/cat1.txt": [8, 9, 10],
        "/some/path/cat2.txt": [13, 14, 16],
        "/some/path/cat5.txt": [31, 39, 42, 48, 52],
    }
}

*/