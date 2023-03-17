namespace thsearch;


interface IIndex
{
    void Add(string path, FileIndexEntry entry);

    void Prune(List<string> foundFiles);

    bool FileUpToDate(FileModel file);

    int GetFileCount();

    string GetPath(int fileId);

    void Save();

    bool TryLookUpToken(string token, out IDictionary<int, List<int>> ranksDictionary);

}