namespace thsearch;

using System.Collections.Concurrent;
using System.Text.Json;
using System.IO;
using System.Diagnostics;


// Represents and operates on the FileIndex and the InverseIndex. The InverseIndex is always downstream from the FileIndex. 

class Index
{
    // int pathId : FileIndexEntry entry
    public ConcurrentDictionary<int, FileIndexEntry> FileIndex { get; }

    public ConcurrentDictionary<
        string, ConcurrentDictionary<
            int, List<int>
            >
        > 
    InverseIndex { get; }

    public ConcurrentDictionary<string, int> IdIndex { get; }

    private string fileIndexPath;
    private string inverseIndexPath;
    private string idIndexPath;


    /// <summary>
    ///  If fileIndex.json and inverseIndex.json exist and are not empty, deserialize them, otherwise make new ones
    /// </summary>
    /// <param name="fileIndexPath">a path to a json file</param>
    /// <param name="inverseIndexPath">a path to a json file<</param>

    public Index(string fileIndexPath, string inverseIndexPath, string idIndexPath)
    {

        this.fileIndexPath = fileIndexPath;
        this.inverseIndexPath = inverseIndexPath;
        this.idIndexPath = idIndexPath;

        // Try to deserialize the fileIndex and inverseIndex, if they exist and are not empty. Otherwise create new ones

        try 
        {
            this.FileIndex = JsonSerializer.Deserialize<
                ConcurrentDictionary<
                    int, FileIndexEntry
                >
            >
            (
                File.ReadAllText(this.fileIndexPath)
            );
        }
        catch (Exception ex) 
        {
            Debug.WriteLine($"Error deserializing file {this.fileIndexPath}: {ex}");
            this.FileIndex = new ConcurrentDictionary<int, FileIndexEntry>();
        }

        try 
        {
            this.InverseIndex = JsonSerializer.Deserialize<
                ConcurrentDictionary<
                    string, ConcurrentDictionary<
                        int, List<int>
                    >
                >
            > 
            (
                File.ReadAllText(this.inverseIndexPath)
            );
        
        }
        catch (Exception ex) 
        {
            Debug.WriteLine($"Error deserializing file {this.inverseIndexPath}: {ex}");
            this.InverseIndex = new ConcurrentDictionary<
                string, ConcurrentDictionary<
                    int, List<int>
                >
            >();
        }
        
        try 
        {
            this.IdIndex = JsonSerializer.Deserialize<ConcurrentDictionary<string, int>>
            (
                File.ReadAllText(this.idIndexPath)
            );
        }
        catch (Exception ex) 
        {
            Debug.WriteLine($"Error deserializing file {this.idIndexPath}: {ex}");
            this.IdIndex = new ConcurrentDictionary<string, int>();
        }

    }

    // TODO: change paths and words to be number ids to reduce lookup time and index size.
    //  - A dictionary can store pathIds and and wordIds. When searching, only on match need to reveal pathId.
    // TODO: Use some compression to save read and write time. Read, decompress, deserialize.
    // TODO: deserialize to binary

    // TODO: benchmarking!

    /// <summary>
    /// Updates the downstream dependant InverseIndex at the same time. A FileIndexEntry also contains everything needed for a InverseIndexEntry
    /// </summary>
    public void Add(string path, FileIndexEntry entry)
    {

        
        int pathId = GetPathId(path);
            

        // key, value, update func
        this.FileIndex.AddOrUpdate(pathId, entry, (k, v) => entry);

        //Neded for updates. Add only runs on new or updated files:
        RemovePathIdFromInverseIndex(pathId);

        // Update the InverseIndex
        for(var i = 0; i < entry.Stems.Length; i++)
        {
            this.InverseIndex.AddOrUpdate(
                // If stem doesn't exist in the InverseIndex, create it and its Ranks Dictionary.
                // key to update
                entry.Stems[i],
                // If it's not in dictionary add it using this function:
                (kStem) =>
                {
                    var ranksDict = new ConcurrentDictionary<int, List<int>>();
                    ranksDict.TryAdd(pathId, new List<int>() { i });
                    return ranksDict;
                },
                // If it is in dictionary update it using this function:
                (kStem, vRanksDictionary) =>
                {
                    // For it's nested ConcurrentDictionary (ranks dictionary), also AddOrUpdate
                    vRanksDictionary.AddOrUpdate(
                        pathId,
                        (key) =>
                        {
                            return new List<int>() { i };
                        },
                        (kPathId, vRanks) =>
                        {
                            vRanks.Add(i);
                            return vRanks;
                        }
                    );
                    return vRanksDictionary;
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
        int[] foundFilesIds = foundFiles.Select( path => 
        {
            this.IdIndex.TryGetValue(path, out int id);
            return id;
        })
        .ToArray();

        foreach (int pathId in this.FileIndex.Keys.Except(foundFilesIds))
        {
            this.Remove(pathId);
        }
    }


    public void Remove(int pathId)
    {
        // Remove from the FileIndex
        this.FileIndex.TryRemove(pathId, out FileIndexEntry entry);

        // Remove from the InverseIndex
        // Iterates over all all the keys (stems) of InverseIndex, and where the stem's RankDict contains key matching the path, it will remove that dictionary item
        RemovePathIdFromInverseIndex(pathId);
    }

    private void RemovePathIdFromInverseIndex(int pathId)
    {
        foreach (string word in this.InverseIndex.Keys)
        {
            if (this.InverseIndex.TryGetValue(word, out ConcurrentDictionary<int, List<int>> rankDict))
            {
                rankDict.Remove(pathId, out List<int> ranks);
            }

            // If the word's rank dictionary is now empty, delete the word
            if (this.InverseIndex.TryGetValue(word, out rankDict) && rankDict.Count == 0)
            {
                this.InverseIndex.Remove(word, out ConcurrentDictionary<int, List<int>> _);
            }
        }
    }

    public void Save()
    {
        File.WriteAllText(this.fileIndexPath, JsonSerializer.Serialize(this.FileIndex));
        File.WriteAllText(this.inverseIndexPath, JsonSerializer.Serialize(this.InverseIndex));
        File.WriteAllText(this.idIndexPath, JsonSerializer.Serialize(this.IdIndex));
    }

    public bool FileUpToDate(FileModel file)
    {
        this.IdIndex.TryGetValue(file.Path, out int pathId);

        return this.FileIndex.ContainsKey(pathId) && file.LastModified <= this.FileIndex[pathId].LastModified;
    }
    
    // Check if path exists in this.IdIndex, if yes return it, otherwise add it and return the value
    public int GetPathId(string path)
    {
        if (this.IdIndex.TryGetValue(path, out int pathId))
        {
            return pathId;
        }
        else
        {
            pathId = this.IdIndex.Count;
            this.IdIndex.TryAdd(path, pathId);
            return pathId;
        }
    }

    

}