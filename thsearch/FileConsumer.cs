namespace thsearch;

using System.IO;
using static System.Net.WebRequestMethods;
using System.Diagnostics;



/// <summary>
/// Concurrently consumes file paths, and adds distributes their contents into an index
/// </summary>
class FileConsumer {
    private IIndex index;
    private StringExtractor stringExtractor;
    private Tokenizer tokenizer;

    public FileConsumer(IIndex index, StringExtractor stringExtractor, Tokenizer tokenizer) {
        this.index = index;
        this.stringExtractor = stringExtractor;
        this.tokenizer = tokenizer;
    }

    public void Consume(FileModel file) {

        Stopwatch stopwatch = new Stopwatch();

        // TODO: Not properly returning when file is up to date
        if (this.index.RecordUpToDate(file)) return;
        
        stopwatch.Start(); // !START

        string rawString = stringExtractor.Extract(file.Path, Path.GetExtension(file.Path));
        string[] stems = tokenizer.Process(rawString);

        stopwatch.Stop(); // STOP
        var extractAndStemTime = stopwatch.ElapsedMilliseconds;
        stopwatch.Reset();

        stopwatch.Start(); // !START

        FileIndexEntry entry = new FileIndexEntry(file.LastModified, stems);
        this.index.Add(file.Path, entry);


        stopwatch.Stop(); // STOP
        var addingTime = stopwatch.ElapsedMilliseconds;
        stopwatch.Reset();

        Console.WriteLine($"Extracting and stemming: {extractAndStemTime}ms, Adding to index: {addingTime}ms");
    
    }
}