namespace thsearch;

using System.IO;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Enumerables;



/// <summary>
/// Concurrently consumes file paths, and adds distributes their contents in the FileIndex and InverseIndex in memory
/// </summary>
class FileConsumer {
    private FileIndex fileIndex;
    private InverseIndex inverseIndex;
    private StringExtractor stringExtractor;
    private Tokenizer tokenizer;
    private Stemmer stemmer;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="fileIndex">the file index, a concurrent dictionary, of path and tokens</param>
    /// <param name="inverseIndex">the opposite of the file index, a concurrent dictionary, of tokens and paths</param>
    /// <param name="stringExtractor">A type that extracts the string from a file path and file type argument</param>
    /// <param name="tokenizer">Splits the string into tokens, FileConsumer expects this to return a ReadOnlySpanTokenizer<char> return value"</param>
    /// <param name="stemmer">Stems words, FileConsumer expects it to return a ReadOnlySpan<string></param>

    public FileConsumer(FileIndex fileIndex, InverseIndex inverseIndex, StringExtractor stringExtractor, Tokenizer tokenizer, Stemmer stemmer) {
        this.fileIndex = fileIndex;
        this.inverseIndex = inverseIndex;
        this.stringExtractor = stringExtractor;
        this.tokenizer = tokenizer;
        this.stemmer = stemmer;
    }

    public void Process(FileModel file) {
    
        if (fileIndex.Contains(file.Path) && file.LastModified <= fileIndex[file.Path].LastModified) { return; }


        string text = stringExtractor.Extract(file.Path, Path.GetExtension(file.Path));
 
        ReadOnlySpanTokenizer<Char> tokens = tokenizer.Tokenize(text);
      
        ReadOnlySpan<string> stems = stemmer.Stem(tokens);

        FileIndexEntry entry = new 
  
        fileIndex.Add(file.Path, file.LastModified, stems);
  
        inverseIndex.Add(file.Path, stems);    
    }
}