namespace thsearch;

using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

class Program
{
    internal static void Main(string[] args)
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
                while (!filesQueue.IsCompleted)
                {
                    if (filesQueue.TryTake(out string file, Timeout.Infinite))
                    {
                        // Search for the search string in the file
                        if (FileContainsString(file, searchString))
                        {
                            Console.WriteLine(file);
                        }
                    }
                }
            });
        }

        // Wait for the producer task to complete
        producerTask.Wait();

        // Wait for the consumer tasks to complete
        Task.WaitAll(consumerTasks);
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