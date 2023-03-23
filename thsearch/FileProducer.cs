namespace thsearch;
using System.Collections;


// FileProducer is a generator that is initialized with three lists of strings: included directories, excluded directories and file extensions. It will use the list of file extensions to find suitable files in the included directories (which it searches recursively). It will check the path if string.Contains an element from excluded directories.

class FileProducer : IEnumerable<FileModel>
{
    private List<string> includedDirectories;
    private List<string> excludedDirectories;
    private List<string> excludedWords;
    private List<string> fileExtensions;

    public FileProducer(List<string> includedDirectories, List<string> excludedDirectories, List<string> fileExtensions, List<string> excludedWords)
    {
        this.includedDirectories = includedDirectories;
        this.excludedDirectories = excludedDirectories;
        this.excludedWords = excludedWords;
        this.fileExtensions = fileExtensions;
    }

    public IEnumerator<FileModel> GetEnumerator()
    {
        foreach (string directory in includedDirectories)
        {
            foreach (string filePath in FindFilesGently(directory))
            {
                if (fileExtensions.Any(filePath.EndsWith))
                {
                    if (!excludedDirectories.Any(filePath.Contains) && !excludedWords.Any(filePath.Contains))
                    {
                        yield return new FileModel(filePath);
                    }
                }
            }
            
        }
    }

    //see: https://stackoverflow.com/a/65059596, for why this is necessary
    private IEnumerable<string> FindFilesGently(string directory)
    {
        IEnumerator<string> fEnum;
        try
        {
            fEnum = Directory.EnumerateFiles(directory, "*.*", SearchOption.AllDirectories).GetEnumerator();
        }
        catch (UnauthorizedAccessException) { yield break; }
        while (true)
        {
            try { if (!fEnum.MoveNext()) break; }
            catch (UnauthorizedAccessException) { continue; }
            yield return fEnum.Current;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

