namespace thsearch;


interface IIndex
{
    void Add(string path, FileIndexEntry entry);

    // Deletes all entries in the Files and Stems table where the path is not in foundFiles
    void Prune(List<string> foundFiles);

    // If file exists in the Files and is not out of date, return true
    bool FileUpToDate(FileModel file);

    // Total files in the File table
    int GetFileCount();

    // Get the path of the file for a given id
    string GetPath(int fileId);

    // takes a stem and if it exists in the stems table, will return an array of tuples of fileIds to occurances.
    // TODO: Update searcher and Index.cs to accept this new method signature
    bool TryLookUpStem(string stem, out List<(int, int)> occurances);

    void Finished();
}