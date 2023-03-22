namespace thsearch;


interface IIndex
{
    void Add(string path, FileIndexEntry entry);

    // Compare the files that were actually found vs what we have saved in the index. Some parts of the index might need snipping off
    void Prune(List<string> foundFiles);

    // If file exists in the Files and is not out of date, return true
    bool RecordUpToDate(FileModel file);

    // Total files in the File table
    int GetFileCount();

    // Get the path of the file for a given id
    string GetPath(int fileId);

    // takes a stem and if it exists in the stems table, will return an array of tuples of fileIds to occurances.
    bool TryLookUpStem(string stem, out List<(int, int)> occurances);

    void Finished();
}