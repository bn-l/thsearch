
namespace thsearch
{


    class ConfigFileParser
    {
        public List<string> IncludedDirectories { get; private set; }
        public List<string> ExcludedDirectories { get; private set; }
        public List<string> FileExtensions { get; private set; }

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
                    else if (line.StartsWith("+"))
                    {
                        IncludedDirectories.Add(Path.GetFullPath(line.Substring(1)));
                    }
                    else if (line.StartsWith("-"))
                    {
                        ExcludedDirectories.Add(Path.GetFullPath(line.Substring(1)));
                    }
                    else if (line.StartsWith(">"))
                    {
                        FileExtensions.Add(line.Substring(1));
                    }
                }
            }

            if (!IncludedDirectories.Any() || !FileExtensions.Any())
            {
                Console.WriteLine("Need at least one included directory and one extension in your thsearch.txt");
                Environment.Exit(1);
            }
        }
    }



}


