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
    private ITokenizer tokenizer;

    public FileConsumer(IIndex index, StringExtractor stringExtractor, ITokenizer tokenizer) {
        this.index = index;
        this.stringExtractor = stringExtractor;
        this.tokenizer = tokenizer;
    }

    public void Consume(FileModel file) {

        Stopwatch stopwatch = new Stopwatch();

        if (this.index.RecordUpToDate(file)) return;
        
        stopwatch.Start(); // !START

        string rawString = stringExtractor.Extract(file.Path, Path.GetExtension(file.Path));

        stopwatch.Stop(); // STOP
        var extractTime = stopwatch.ElapsedMilliseconds;
        stopwatch.Reset();

        stopwatch.Start(); // !START

        List<string> stems = tokenizer.Process(rawString);

        stopwatch.Stop(); // STOP
        var stemTime = stopwatch.ElapsedMilliseconds;
        stopwatch.Reset();

        stopwatch.Start(); // !START

        FileIndexEntry entry = new FileIndexEntry(file.LastModified, stems);
        this.index.Add(file.Path, entry);


        stopwatch.Stop(); // STOP
        var addingTime = stopwatch.ElapsedMilliseconds;
        stopwatch.Reset();

        Console.WriteLine($"Extracting: {extractTime}ms, Stemming: {stemTime}ms, Adding to index: {addingTime}ms");
    
    }
}