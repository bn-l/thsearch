using System.Collections.Concurrent;
using System.Reflection;
using thsearch;

class Program
{
    static void Main(string[] args)
    {

        string searchString;
        string configPath;
        string configName = "thsearch.txt";

        if (args.Length == 0)
        {
            Console.WriteLine("\nUsage: thsearch.exe <search string> <config file> \n\n Note: Config file defaults to thsearch.txt in the same directory as the exe \n\n Config format (each on new line):\n\n   +C:\\some dir to included\\\n   -C:\\some dir to exclude\\\n   >ext\n\n");
            return;
        }
        else if (args.Length == 1)
        {
            searchString = args[0];
            string currentDirectory = Path.GetDirectoryName(AppContext.BaseDirectory);
            configPath = Path.Combine(currentDirectory, configName);
        }
        else
        {
            searchString = args[0];
            configPath = args[1];
        }
        var config = new ConfigFileParser(configPath);
        // rest of the script remains the same

        // Console.WriteLine("searchString: " + searchString);
        // Console.WriteLine("Included directories: " + string.Join(", ", config.IncludedDirectories));
        // Console.WriteLine("Excluded directories: " + string.Join(", ", config.ExcludedDirectories));
        // Console.WriteLine("Extensions: " + string.Join(", ", config.FileExtensions));

        // Create a shared queue for the files
        ConcurrentQueue<string> filesQueue = new ConcurrentQueue<string>();

        bool threadsRunning = true;

        // Create the producer thread
        var producerThread = new Thread(() => {
            // Search for matching file types and add them to the queue
            foreach (string directory in config.IncludedDirectories)
            {
                if (!config.ExcludedDirectories.Contains(directory))
                {
                    var matchingFiles = GetMatchingFiles(directory, config.FileExtensions, config.ExcludedDirectories);
                    foreach (string file in matchingFiles)
                    {
                        filesQueue.Enqueue(file);
                    }
                }
            }
            threadsRunning = false;
        });

        // Start the producer thread
        producerThread.Start();

        // Get the number of available processors
        int processorCount = Environment.ProcessorCount-1 | 1;

        // Create the consumer threads
        var consumerThreads = new List<Thread>();
        for (int i = 0; i < processorCount; i++)
        {
            var consumerThread = new Thread(() => {
                while (threadsRunning || !filesQueue.IsEmpty)
                {
                    if (filesQueue.TryDequeue(out string file))
                    {
                        // Search for the search string in the file
                        if (FileContainsString(file, searchString))
                        {
                            Console.WriteLine(file);
                        }
                    }

                }

            });
            consumerThread.Start();
            consumerThreads.Add(consumerThread);
        }

        // Wait for the producer thread to complete
        producerThread.Join();

        // Wait for the consumer threads to complete
        foreach (var consumerThread in consumerThreads)
        {
            consumerThread.Join();
        }

    }

    static bool FileContainsString(string file, string searchString)
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

    static List<string> GetMatchingFiles(string directory, List<string> fileExtensions, List<string> excludedDirectories)
    {
        var matchingFiles = new List<string>();
        foreach (string file in Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories))
        {
            string extension = Path.GetExtension(file);
            if (fileExtensions.Contains(extension))
            {
                var fileDirectory = Path.GetDirectoryName(file);
                if (!excludedDirectories.Contains(fileDirectory))
                {
                    matchingFiles.Add(file);
                }
            }
        }
        return matchingFiles;
    }

}
