namespace thsearch;

using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Globalization;
using System.Diagnostics;

class Program
{

    internal static void Main(string[] args)
    {

        string searchString;
        string configName = "thsearch";
        string configPath;
        string currentDirectory = Path.GetDirectoryName(AppContext.BaseDirectory);
        int numberOfResults = 10;

        // TODO: Tests
        // TODO: File access issues

        switch (args.Length)
        {
            // without args
            case 0:
                Console.WriteLine("\nUsage: thsearch.exe <search string> <config file> \n\n Note: Config file defaults to thsearch.txt in the same directory as the exe \n\n Config format (each on new line):\n\n   +C:\\some dir to included\\\n   -C:\\some dir to exclude\\\n   >ext\n\n");
                return;
            // only a search string arg
            case 1: 
                searchString = args[0];

                break;
            // specifying a config name. Or none to use the default (and optionally specify "all" results)
            case 2:
                searchString = args[0];
                if (args[1] == "all") 
                {
                    numberOfResults = -1;
                }
                else {
                    configName = args[1];
                }
                break;
            case 3:
                searchString = args[0];
                configName = args[1];
                if (args[2] == "all") 
                {
                    numberOfResults = -1;
                }
                break;
            default:
                Console.WriteLine("Error: Too many arguments");
                return;                
        }


        configPath = Path.Combine(currentDirectory, configName + ".txt");

        ConfigFileParser config = new ConfigFileParser(configPath);

        string dbLocation = string.IsNullOrEmpty(config.DBLocation) ? 
            Path.Combine(currentDirectory, configName + ".sqlite") :
            Path.Combine(config.DBLocation, configName + ".sqlite");

        IIndex index = new IndexSqlite(dbLocation);


        TxtExtractor txtExtractor = new TxtExtractor();
        PdfExtractor pdfExtractor = new PdfExtractor();
        HtmlExtractor htmlExtractor = new HtmlExtractor();
        EpubExtractor epubExtractor = new EpubExtractor();
        StringExtractor stringExtractor = new StringExtractor(new IExtractor[] { txtExtractor, pdfExtractor, htmlExtractor, epubExtractor }, txtExtractor);

        ITokenizer tokenizer = new TokenizerSpans();

        FileProducer fileProducer = new FileProducer (config.IncludedDirectories, config.ExcludedDirectories, config.FileExtensions, config.ExcludedWords);
        

        FileConsumer fileConsumer = new FileConsumer(index, stringExtractor, tokenizer);

        BlockingCollection<FileModel> filesQueue = new BlockingCollection<FileModel>();

        // used for pruning later
        List<string> foundFiles = new List<string>();


        var producerTask = Task.Run(() =>
        {
            foreach (FileModel file in fileProducer)
            {
                filesQueue.Add(file);
            }
            filesQueue.CompleteAdding();
        });

        int processorCount = Environment.ProcessorCount - 1 | 1;
        var consumerTasks = new Task[processorCount];

        for (int i = 0; i < processorCount; i++)
        {
            consumerTasks[i] = Task.Run(() =>
            {
                // from the doc: "The enumerator will continue to provide items (if any exist) until IsCompleted returns true"
                //  if IsCompleted is false the loop blocks until an item becomes
                foreach (FileModel file in filesQueue.GetConsumingEnumerable())
                {

                    if(fileConsumer.Consume(file)) foundFiles.Add(file.Path);
                }
            });
        }


        Stopwatch stopwatch = new Stopwatch();

        stopwatch.Start(); // !START

        Task.WaitAll(producerTask, Task.WhenAll(consumerTasks));

        Console.WriteLine($"It took {stopwatch.ElapsedMilliseconds} ms to process all files");
        stopwatch.Reset(); // RESET 


        stopwatch.Start(); // !START

        index.Finished();


        stopwatch.Start(); // !START


        index.Prune(foundFiles);
    
        Console.WriteLine($"It took {stopwatch.ElapsedMilliseconds} ms to prune");
        stopwatch.Reset(); // RESET 

        Searcher searcher = new Searcher(tokenizer);
        

        stopwatch.Start();  // !START

        List<string> results = searcher.TfIdf(index, searchString).Select(id => index.GetPath(id)).ToList();

        if (numberOfResults == -1)
        {
            foreach (string result in results)
            {
                Console.WriteLine(result);
            }
        }
        else
        {
            for (int i = 0; i < numberOfResults && i < results.Count; i++)
            {
                Console.WriteLine(results[i]);
            }
        }


        Console.WriteLine($"It took {stopwatch.ElapsedMilliseconds} ms to search (according to Program.cs)");
        stopwatch.Reset(); // RESET 

    }


}