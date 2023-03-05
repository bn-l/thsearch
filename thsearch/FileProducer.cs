namespace thsearch;
using System.Collections;


// FileProducer is a generator that is initialized with three lists of stringss: included directories, excluded directories and file extensions. It will use the list of file extensions to find suitable files in the included directories (which it searches recursively). It will check the path if string.Contains an element from excluded directories.

class FileProducer : IEnumerable<FileModel>
{
    private List<string> includedDirectories;
    private List<string> excludedDirectories;
    private List<string> fileExtensions;

    public FileProducer(List<string> includedDirectories, List<string> excludedDirectories, List<string> fileExtensions)
    {
        this.includedDirectories = includedDirectories;
        this.excludedDirectories = excludedDirectories;
        this.fileExtensions = fileExtensions;
    }

    public IEnumerator<FileModel> GetEnumerator()
    {
        foreach (string directory in includedDirectories)
        {
            foreach (string fileMatch in Directory.EnumerateFiles(directory, "*.*", SearchOption.AllDirectories))
            {
                if (fileExtensions.Any(fileMatch.EndsWith))
                {
                    if (!excludedDirectories.Any(fileMatch.Contains))
                    {
                        yield return new FileModel(fileMatch);
                    }
                }
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

