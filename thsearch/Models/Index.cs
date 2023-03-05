namespace thsearch;

using System.Collections.Concurrent;
using System.Text.Json;
using System.IO;


// Represents and operates on the FileIndex and the InverseIndex. The InverseIndex is always downstream from the FileIndex. 

class Index 
{
    // string path : FileIndexEntry entry
    private ConcurrentDictionary<string, FileIndexEntry> fileIndex;
    // string stem : InverseIndexEntry entry
    private ConcurrentDictionary<string, InverseIndexEntry> inverseIndex;
    private string fileIndexPath;
    private string inverseIndexPath;

    /// <summary>
    ///  Looks worse than it is: In this contructor if fileIndex.json and inverseIndex.json do not exist it will create them, otherwise it deserializes each
    /// </summary>
    /// <param name="fileIndexPath">a path to a json file</param>
    /// <param name="inverseIndexPath">a path to a json file<</param>
    public Index (string fileIndexPath, string inverseIndexPath) 
    {

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
    public void Add(string path, FileIndexEntry entry) 
    {
        
        // key, value, update func
        this.fileIndex.AddOrUpdate(path, entry, (key, value) => entry);

        // Update the InverseIndex
        foreach (string stem in entry.Stems) 
        {
            this.inverseIndex.AddOrUpdate(
                // If stem doesn't exist in the InverseIndex, create it by k,v:
                stem, 
                new InverseIndexEntry(stem, entry.Stems.IndexOf(stem)),
                // If it does this function updates it:
                (key, value) => 
                {
                    // update the ranks here
                    value.Ranks[path].Add(entry.Stems.IndexOf(stem));
                    return value;
                }
            );
        }

    }

    public void Remove(string pathToRemove) 
    {
        this.fileIndex.TryRemove(pathToRemove, out FileIndexEntry entry);
        // Iterates over all all the keys of InverseIndex, and where the inverseIndexEntry.path == pathToRemove it will remove the path and ranks 

    }


    public void Save() 
    {
        File.WriteAllText(this.fileIndexPath, JsonSerializer.Serialize(this.fileIndex));
        File.WriteAllText(this.inverseIndexPath, JsonSerializer.Serialize(this.inverseIndex));
    }

    
}