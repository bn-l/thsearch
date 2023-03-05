namespace thsearch;

using System.Collections.Concurrent;
using System.Text.Json;
using System.IO;


// Represents and operates on the FileIndex and the InverseIndex. The InverseIndex is always downstream from the FileIndex. 

class Index {
    private ConcurrentDictionary<string, FileIndexEntry> fileIndex;
    private ConcurrentDictionary<string, InverseIndexEntry> inverseIndex;
    private string fileIndexPath;
    private string inverseIndexPath;

    /// <summary>
    ///  In this contructor if fileIndex.json and inverseIndex.json do not exist it will create them, otherwise it deserializes each
    /// </summary>
    /// <param name="fileIndexPath">a path to a json file</param>
    /// <param name="inverseIndexPath">a path to a json file<</param>
    public Index (string fileIndexPath, string inverseIndexPath) {

        this.fileIndexPath = fileIndexPath;
        this.inverseIndexPath = inverseIndexPath;

        if (!File.Exists(this.fileIndexPath)) 
        {
            File.Create(this.fileIndexPath);
            this.fileIndex = new ConcurrentDictionary<string, FileIndexEntry>();
        }
        else 
        {
            this.fileIndex = JsonSerializer.Deserialize<ConcurrentDictionary<string, FileIndexEntry>>(File.ReadAllText(this.fileIndexPath));
        }

        if (!File.Exists(this.inverseIndexPath)) 
        {
            File.Create(this.inverseIndexPath);
            this.inverseIndex = new ConcurrentDictionary<string, InverseIndexEntry>();
        } 
        else 
        {
            this.inverseIndex = JsonSerializer.Deserialize<ConcurrentDictionary<string, InverseIndexEntry>>(File.ReadAllText(this.inverseIndexPath));
        }
    }


    /// <summary>
    /// Updates the downstream dependant InverseIndex at the same time. A FileIndexEntry also contains everything needed for a InverseIndexEntry
    /// </summary>
    public void Add(FileIndexEntry entry) {
        
        this.fileIndex.AddOrUpdate(path, entry, (key, value) => entry);

        // Iterates over entry entry[path].stems and AddOrUpdates each this.inverseIndex[stem] by accessing the path and the adding the array index of the stem to the ranks list
        foreach (var stem in entry.stems) {
            this.inverseIndex.AddOrUpdate(stem, new InverseIndexEntry(
                new Dictionary<string, InverseIndexValue> {
                    
                }
            )
        }



    }

    public void Save() {
        File.WriteAllText(this.path, JsonSerializer.Serialize(this.index));
    }

    
}