namespace Tests;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using Tests.Helpers;
using thsearch;

[TestClass]
public class Thsearch_function_tests
{

    private List<string> _foundFiles, _foundExtensions, _includedFiles,_excludedFiles, _includedDirectories, _excludedDirectories;

    public TestContext TestContext { get; set; }

    [TestInitialize]
    public void Initialize()
    {
        // Scans the TestData folder for all files. If a folder contains the word "Include" (case sensitive) it goes into the included folders list, and vica versa for exclude. It also adds *all* found extensions to the included extensions list.
        // It will build therefore, a new config file depending on the TestData folder and allows for another test class to build more complex test file structures
        // Each text file should have the string "test" somewhere

        _foundFiles = Directory.GetFiles("TestData", "*.*", SearchOption.AllDirectories).ToList();
        _includedFiles = _foundFiles.Where(f => f.Contains("Include")).ToList();
        _excludedFiles = _foundFiles.Where(f => f.Contains("Exclude")).ToList();

        // Add the extensions of the found files to the foundExtensions list
        _foundExtensions = _foundFiles.Select(f => Path.GetExtension(f)).Distinct().ToList();
        // Create the thsearch.txt file
        var configFile = File.CreateText("thsearch.txt");

        // Using the included files list, write directory include lines
        _includedDirectories = _includedFiles.Select(Path.GetDirectoryName).Select(Path.GetFullPath).Distinct().ToList();
        _includedDirectories.ForEach(f => configFile.WriteLine("+" + f));

        // Using the excluded files list, write directory exclude lines
        _excludedDirectories = _excludedFiles.Select(Path.GetDirectoryName).Select(Path.GetFullPath).Distinct().ToList();
        _excludedDirectories.ForEach(f => configFile.WriteLine("-" + f));

        // Add the file extensions to the thsearch.txt file
        _foundExtensions.ForEach(e => configFile.WriteLine(">" + e));

        // Close the thsearch.txt file
        configFile.Close();
    }

    [TestMethod]
    public void ConfigFileParser_ShouldHaveCorrectIncludedDirectories()
    {
        // Arrange
        List<string> testGeneratedIncludedDirs = _includedDirectories;
        ConfigFileParser config = new ConfigFileParser("thsearch.txt");

        // Act
        List<string> configIncludedDirs = config.IncludedDirectories;

        // Assert
        CollectionAssert.AreEqual(testGeneratedIncludedDirs, configIncludedDirs);
    }

    [TestMethod]
    public void ConfigFileParser_ShouldHaveCorrectExcludedDirectories()
    {
        // Arrange
        List<string> testGeneratedExcludedDirs = _excludedDirectories;
        ConfigFileParser config = new ConfigFileParser("thsearch.txt");

        // Act
        List<string> configExcludedDirs = config.ExcludedDirectories;

        // Assert
        CollectionAssert.AreEqual(testGeneratedExcludedDirs, configExcludedDirs);
    }



    [TestMethod]
    public void Main_WithIncludedFile_ShouldPrintIncludedFiles()
    {
        // Arrange
        string searchString = "test";
        string configPath = "thsearch.txt";

        List<string> expectedOutput = _includedFiles.Select(Path.GetFullPath).ToList();

        // Expected that the main method will add a new line character after the last path. Using split below this gets converted to an empty string at the end of the list. Let's emulate that:
        expectedOutput.Add("");

        // Act
        using (ConsoleOutput consoleOutput = new ConsoleOutput())
        {
            Program.Main(new string[] { searchString, configPath });
            List<string> output = consoleOutput.GetOutput().Split(Environment.NewLine).ToList();

            // Assert - Currently not working because threads, unlike expectedOutput can deliver paths in any order. This assertion needs to be order independent
            CollectionAssert.AreEquivalent(expectedOutput, output);
        }
    }

    [TestMethod]
    public void Main_PerformanceTest()
    {
        // Arrange
        var searchString = "test";
        var configPath = "thsearch.txt";

        // Start the stopwatch
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        // Act
        Program.Main(new string[] { searchString, configPath });

        // Stop the stopwatch
        stopwatch.Stop();

        // Write the elapsed time to the test results
        TestContext.WriteLine($"Elapsed time: {stopwatch.ElapsedMilliseconds}ms");
    }

    [TestMethod]
    public void TestFileContainsString()
    {
        // Arrange
        string file = Path.GetFullPath(_includedFiles[0]);
        string searchString = "test";

        // Act
        bool result = Program.FileContainsString(file, searchString);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void TestGetMatchingFiles()
    {
        // Arrange
        string directory = Path.GetFullPath(_includedDirectories[0]);
        List<string> fileExtensions = _foundExtensions;
        List<string> excludedDirs = _excludedDirectories.Select(Path.GetFullPath).ToList();

        List<string> expectedOutput = Directory.GetFiles(directory).ToList();

        // Act
        List<string> output = Program.GetMatchingFiles(directory, fileExtensions, excludedDirs);

        // Assert
        CollectionAssert.AreEquivalent(expectedOutput, output);
    }
}