namespace thsearch;

using System.IO;



/// <summary>
/// Concurrently consumes file paths, and adds distributes their contents into an index
/// </summary>
class FileConsumer {
    private Index index;
    private StringExtractor stringExtractor;
    private TokenizerAndStemmer tokenizerAndStemmer;

    public FileConsumer(Index index, StringExtractor stringExtractor, TokenizerAndStemmer tokenizerAndStemmer) {
        this.index = index;
        this.stringExtractor = stringExtractor;
        this.tokenizerAndStemmer = tokenizerAndStemmer;
    }

    public void Consume(FileModel file) {
    
        // Do we have it? And if yes, is ours out of date?
        if (index.FileIndex.ContainsKey(file.Path) && file.LastModified <= index.FileIndex[file.Path].LastModified) { return; }


        string rawString = stringExtractor.Extract(file.Path, Path.GetExtension(file.Path));
        
        
        List<string> stems = tokenizerAndStemmer.Process(rawString);

        FileIndexEntry entry = new FileIndexEntry(file.LastModified, stems);

        this.index.Add(file.Path, entry);
    
    }
}