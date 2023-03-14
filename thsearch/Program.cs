namespace thsearch;

using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Globalization;

class Program
{

    internal static void Main(string[] args)
    {

        string searchString;
        string configName = "thsearch";
        string configPath;
        string currentDirectory = Path.GetDirectoryName(AppContext.BaseDirectory);

        // !!! TODO: Remove stop words as well


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
            // specifying a config name 
            case 2:
                searchString = args[0];
                configName = args[1];

                break;
            default:
                Console.WriteLine("Error: Too many arguments");
                return;                
        }

        configPath = Path.Combine(currentDirectory, configName + ".txt");
        ConfigFileParser config = new ConfigFileParser(configPath);

        Index index = new Index(Path.Combine(currentDirectory, "fileIndex.json"), Path.Combine(currentDirectory, "inverseIndex.json"));


        TxtExtractor txtExtractor = new TxtExtractor();
        PdfExtractor pdfExtractor = new PdfExtractor();
        HtmlExtractor htmlExtractor = new HtmlExtractor();
        EpubExtractor epubExtractor = new EpubExtractor();
        StringExtractor stringExtractor = new StringExtractor(new IExtractor[] { txtExtractor, pdfExtractor, htmlExtractor, epubExtractor }, txtExtractor);

        Tokenizer tokenizer = new Tokenizer();

        FileProducer fileProducer = new FileProducer (config.IncludedDirectories, config.ExcludedDirectories, config.FileExtensions);

        FileConsumer fileConsumer = new FileConsumer(index, stringExtractor, tokenizer);


        BlockingCollection<FileModel> filesQueue = new BlockingCollection<FileModel>();
        // used for pruning later
        List<string> foundFiles = new List<string>();


        // Create the producer task
        var producerTask = Task.Run(() =>
        {
            // file producer is a generator that produces a File
            foreach (FileModel file in fileProducer)
            {
                filesQueue.Add(file);
                foundFiles.Add(file.Path);
            }
            filesQueue.CompleteAdding();
        });

        // Get the number of available processors
        int processorCount = Environment.ProcessorCount - 1 | 1;

        // FileConsumer instance called with the Index and InverseIndex instances as references

        // Create the consumer tasks
        var consumerTasks = new Task[processorCount];
        for (int i = 0; i < processorCount; i++)
        {
            consumerTasks[i] = Task.Run(() =>
            {
                // from the doc: "The enumerator will continue to provide items (if any exist) until IsCompleted returns true"
                //  if IsCompleted is false the loop blocks until an item becomes
                foreach (FileModel file in filesQueue.GetConsumingEnumerable())
                {
                    // Search for the search string in the file
                    fileConsumer.Consume(file);
                }
            });
        }

        // Wait for the producer task to complete
        // producerTask.Wait();

        Task.WaitAll(producerTask, Task.WhenAll(consumerTasks));

        // Compare the files that were actually found vs what we have saved in the index. Some parts of the index might need snipping off
        index.Prune(foundFiles);

        // Index has been updated. Let's save it to disk.
        index.Save();

        Searcher searcher = new Searcher(tokenizer);


        foreach (string result in searcher.TfIdf(index, searchString))
        {
            Console.WriteLine(result);
        }

    }


}