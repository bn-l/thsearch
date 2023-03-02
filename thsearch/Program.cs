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

        // Create a shared BlockingCollection for the files
        var filesQueue = new BlockingCollection<string>();

        // Create the producer task
        var producerTask = Task.Run(() =>
        {
            // Search for matching file types and add them to the queue
            foreach (string directory in config.IncludedDirectories)
            {
                var matchingFiles = GetMatchingFiles(directory, config.FileExtensions, config.ExcludedDirectories);
                foreach (string file in matchingFiles)
                {
                    filesQueue.Add(file);
                }
            }
            filesQueue.CompleteAdding();
        });

        // Get the number of available processors
        int processorCount = Environment.ProcessorCount - 1 | 1;

        // Create the consumer tasks
        var consumerTasks = new Task[processorCount];
        for (int i = 0; i < processorCount; i++)
        {
            consumerTasks[i] = Task.Run(() =>
            {
                // from the doc: "The enumerator will continue to provide items (if any exist) until IsCompleted returns true"
                //  if IsCompleted is false the loop blocks until an item becomes
                foreach (string file in filesQueue.GetConsumingEnumerable())
                {
                    // Search for the search string in the file
                    if (FileContainsString(file, searchString))
                    {
                        Console.WriteLine(file);
                    }
                }
            });
        }

        // Wait for the producer task to complete
        // producerTask.Wait();

        // Wait for the consumer tasks to complete
        Task.WaitAll(producerTask, Task.WhenAll(consumerTasks));
    }

    internal static bool CustomContains(string source, string toCheck)
    {
        CompareInfo ci = new CultureInfo("en-US").CompareInfo;
        CompareOptions co = CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace;
        return ci.IndexOf(source, toCheck, co) != -1;
    }

    internal static bool FileContainsString(string file, string searchString)

    {
        using (var stream = new StreamReader(file))
        {
            string line;
            while ((line = stream.ReadLine()) != null)
            {
                if (line.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
        }
        return false;
    }

    internal static List<string> GetMatchingFiles(string directory, List<string> fileExtensions, List<string> excludedDirs)
    {
        var matchingFiles = new List<string>();
        foreach (string file in Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories))
        {
            var fileDirectory = Path.GetDirectoryName(file) + Path.DirectorySeparatorChar;

            bool correctExtension = fileExtensions.Contains(Path.GetExtension(file));
            bool pathNotExcluded = !excludedDirs.Any(excludedDir => fileDirectory.Contains(excludedDir + Path.DirectorySeparatorChar));

            if (correctExtension && pathNotExcluded)
            {
                matchingFiles.Add(file);
            }
        }
        return matchingFiles;
    }

}