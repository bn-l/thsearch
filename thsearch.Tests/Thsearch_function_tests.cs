namespace thsearch.Tests;
// file scoped namespace. See: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-10.0/file-scoped-namespaces

using Test.Helpers;



[TestClass]
public class Thsearch_function_tests
{

    private List<string> _foundFiles, _foundExtensions ,_includedFiles,_excludedFiles;

    [TestInitialize]
    public void Initialize()
    {
        _foundFiles = Directory.GetFiles("TestData", "*.*", SearchOption.AllDirectories).ToList();
        _includedFiles = _foundFiles.Where(f => f.Contains("Include")).ToList();
        _excludedFiles = _foundFiles.Where(f => f.Contains("Exclude")).ToList();

        // Add the extensions of the found files to the foundExtensions list
        _foundExtensions = _foundFiles.Select(f => Path.GetExtension(f)).Distinct().ToList();
        // Create the thsearch.txt file
        var configFile = File.CreateText("thsearch.txt");

        // Add the included files to the thsearch.txt file
        _includedFiles.ForEach(f => configFile.WriteLine("+" + f));

        // Add the excluded files to the thsearch.txt file
        _excludedFiles.ForEach(f => configFile.WriteLine("-" + f));

        // Add the file extensions to the thsearch.txt file
        _foundExtensions.ForEach(e => configFile.WriteLine(">" + e));

        // Close the thsearch.txt file
        configFile.Close();
    }

    [TestMethod]
    public void ConfigFileParser_ShouldHaveCorrectIncludedDirectories()
    {
        // Arrange
        List<string> includedDirs = _includedFiles.Select(Path.GetDirectoryName).Distinct().ToList();
        ConfigFileParser config = new ConfigFileParser("thsearch.txt");


        // Act
        List<string> configIncludedDirs = config.IncludedDirectories;

        // Assert
        CollectionAssert.AreEqual(includedDirs, configIncludedDirs);
    }

    [TestMethod]
    public void ConfigFileParser_ShouldHaveCorrectExcludedDirectories()
    {
        // Arrange
        List<string> excludedDirs = _excludedFiles.Select(Path.GetDirectoryName).Distinct().ToList();
        ConfigFileParser config = new ConfigFileParser("thsearch.txt");

        // Act
        List<string> configExcludedDirs = config.ExcludedDirectories;

        // Assert
        CollectionAssert.AreEqual(excludedDirs, configExcludedDirs);
    }


    [TestMethod]
    public void Main_WithIncludedFile_ShouldPrintIncludedFiles()
    {
        // Arrange
        string searchString = "test";
        string configPath = "thsearch.txt";
        string expectedOutput = string.Join(Environment.NewLine, _includedFiles) + Environment.NewLine;

        // Act
        using (ConsoleOutput consoleOutput = new ConsoleOutput())
        {
            Program.Main(new string[] { searchString, configPath });
            string output = consoleOutput.GetOutput();

            // Assert
            Assert.AreEqual(expectedOutput, output);
        }
    }

    [TestMethod]
    public void Main_WithExcludedFile_ShouldNotPrintExcludedFiles()
    {
        // Arrange
        string searchString = "test";
        string configPath = "thsearch.txt";
        string expectedOutput = "";

        // Act
        using (ConsoleOutput consoleOutput = new ConsoleOutput())
        {
            Program.Main(new string[] { searchString, configPath });
            string output = consoleOutput.GetOutput();

            // Assert
            Assert.AreEqual(expectedOutput, output);
        }
    }



}