namespace thsearch;

using System.IO;



/// <summary>
/// Concurrently consumes file paths, and adds distributes their contents into an index
/// </summary>
class FileConsumer {
    private Index index;
    private StringExtractor stringExtractor;
    private Tokenizer tokenizer;

    public FileConsumer(Index index, StringExtractor stringExtractor, Tokenizer tokenizer) {
        this.index = index;
        this.stringExtractor = stringExtractor;
        this.tokenizer = tokenizer;
    }

    public void Consume(FileModel file) {
    
        if(this.index.FileUpToDate(file)) return;
        
        string rawString = stringExtractor.Extract(file.Path, Path.GetExtension(file.Path));
        
        
        string[] stems = tokenizer.Process(rawString);

        FileIndexEntry entry = new FileIndexEntry(file.LastModified, stems);
        
        this.index.Add(file.Path, entry);
    
    }
}