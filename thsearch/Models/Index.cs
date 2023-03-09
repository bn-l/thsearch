namespace thsearch;

using System.Collections.Concurrent;
using System.Text.Json;
using System.IO;


// Represents and operates on the FileIndex and the InverseIndex. The InverseIndex is always downstream from the FileIndex. 

class Index
{
    // string path : FileIndexEntry entry
    public ConcurrentDictionary<string, FileIndexEntry> FileIndex { get; }
    // string stem : InverseIndexEntry entry
    public ConcurrentDictionary<string, InverseIndexEntry> InverseIndex { get; }

    private string fileIndexPath;
    private string inverseIndexPath;


    /// <summary>
    ///  If fileIndex.json and inverseIndex.json exist and are not empty, deserialize them, otherwise make new ones
    /// </summary>
    /// <param name="fileIndexPath">a path to a json file</param>
    /// <param name="inverseIndexPath">a path to a json file<</param>

    public Index(string fileIndexPath, string inverseIndexPath)
    {

        this.fileIndexPath = fileIndexPath;
        this.inverseIndexPath = inverseIndexPath;

        FileInfo fileIndexInfo = new FileInfo(this.fileIndexPath);
        FileInfo inverseIndexInfo = new FileInfo(this.inverseIndexPath);


        if (File.Exists(this.fileIndexPath) && fileIndexInfo.Length > 0)
        {
            this.FileIndex = JsonSerializer.
                Deserialize
                    <ConcurrentDictionary<string, FileIndexEntry>>
                    (File.ReadAllText(this.fileIndexPath));
        }
        else
        {
            this.FileIndex = new ConcurrentDictionary<string, FileIndexEntry>();
        }

        if (File.Exists(this.inverseIndexPath) && inverseIndexInfo.Length > 0)
        {
            this.InverseIndex = JsonSerializer.
                Deserialize
                    <ConcurrentDictionary<string, InverseIndexEntry>>
                    (File.ReadAllText(this.inverseIndexPath));
        }
        else
        {
            this.InverseIndex = new ConcurrentDictionary<string, InverseIndexEntry>();
        }
    }


    /// <summary>
    /// Updates the downstream dependant InverseIndex at the same time. A FileIndexEntry also contains everything needed for a InverseIndexEntry
    /// </summary>
    public void Add(string path, FileIndexEntry entry)
    {

        // key, value, update func
        this.FileIndex.AddOrUpdate(path, entry, (key, value) => entry);

        // Update the InverseIndex
        foreach (string stem in entry.Stems)
        {
            this.InverseIndex.AddOrUpdate(
                // If stem doesn't exist in the InverseIndex, create it by k,v:
                stem,
                new InverseIndexEntry
                (
                    path, new List<int>() { entry.Stems.IndexOf(stem) }
                ),
                // If it does this function updates it:
                (key, value) =>
                {
                    // update the ranks here
                    //             RanksDict:
                    //    path:                 ranks:
                    // "/some/path/dog1.txt": [1, 2, 5, 77, 345],
                    value.RanksDict[path].Add(entry.Stems.IndexOf(stem));
                    return value;
                }
            );
        }

    }

    /// <summary>
    /// iterates over the set difference of keys in fileIndex minus the set of foundFiles, calling Remove(path) for each
    /// </summary>
    /// <param name="foundFiles">A list of found paths</param>
    public void Prune(List<string> foundFiles)
    {

        foreach (string path in this.FileIndex.Keys.Except(foundFiles))
        {
            this.Remove(path);
        }

    }

    public void Remove(string pathToRemove)
    {
        // Remove from the FileIndex
        this.FileIndex.TryRemove(pathToRemove, out FileIndexEntry entry);

        // Remove from the InverseIndex
        // Iterates over all all the keys (stems) of InverseIndex, and where the stem's RankDict contains key matching the path, it will remove that dictionary item
        foreach (var stem in this.InverseIndex)
        {
            if (stem.Value.RanksDict.ContainsKey(pathToRemove))
            {
                stem.Value.RanksDict.TryRemove(pathToRemove, out List<int> ranks);
            }
        }
    }

    public void Save()
    {
        File.WriteAllText(this.fileIndexPath, JsonSerializer.Serialize(this.FileIndex));
        File.WriteAllText(this.inverseIndexPath, JsonSerializer.Serialize(this.InverseIndex));
    }

}