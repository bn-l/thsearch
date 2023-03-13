
namespace thsearch;

// Gives a list of included directories, excluded directories and file extensions from the thsearch.txt file

// TODO: Tests if directory exists. If no, throw exception and exit.

public class ConfigFileParser
{
    public List<string> IncludedDirectories { get; private set; }
    public List<string> ExcludedDirectories { get; private set; }
    public List<string> FileExtensions { get; private set; }

    char dirSeparatorChar = Path.DirectorySeparatorChar;

    public ConfigFileParser(string filePath)
    {
        IncludedDirectories = new List<string>();
        ExcludedDirectories = new List<string>();
        FileExtensions = new List<string>();
        using (var reader = new StreamReader(filePath))
        {
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (line != null)
                {
                    line = line.Trim();
                }
                if (string.IsNullOrWhiteSpace(line))
                {
                    Console.WriteLine("Formatting error, line is empty or whitespace.");
                    Environment.Exit(1);
                }
                if (line.EndsWith(dirSeparatorChar.ToString()))
                {
                    line = line.TrimEnd(dirSeparatorChar);
                }

                if (line.StartsWith("+"))
                {
                    IncludedDirectories.Add(Path.GetFullPath(line.TrimStart('+')));
                }
                else if (line.StartsWith("-"))
                {
                    ExcludedDirectories.Add(Path.GetFullPath(line.TrimStart('-')));
                }
                else if (line.StartsWith(">"))
                {
                    FileExtensions.Add(line.TrimStart('>'));
                }
            }
        }

        if (!IncludedDirectories.Any() || !FileExtensions.Any())
        {
            Console.WriteLine("Error in config: Need at least one included directory and one extension in your thsearch.txt");
            Environment.Exit(1);
        }
        
        // joins and then enumerates IncludedDirectories and ExcludedDirectories, if a directory in either doesn't exist will write a message to the console and exit
        foreach (string directory in IncludedDirectories.Concat(ExcludedDirectories))
        {
            if (!Directory.Exists(directory))
            {
                Console.WriteLine("Error in config: Directory {0} does not exist.", directory);
                Environment.Exit(1);
            }
        }

    }
}





