namespace Tests;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using Tests.Helpers;
using thsearch;

[TestClass]
public class Thsearch_function_tests
{

    private string[] _expectedAbsIncludedDirectories1, _expectedAbsExcludedDirectories1, _expectedAbsIncludedFiles1,_expectedAbsExcludedFiles, _expectedAbsExcludedFiles1;

    private string[] _expectedAbsIncludedDirectories2, _expectedAbsExcludedDirectories2, _expectedAbsIncludedFiles2,_expectedAbsExcludedFiles2;

    public TestContext TestContext { get; set; }

    [TestInitialize]
    public void Initialize()
    {

        #region 1 Test files 1

        // note: matching string in include lowercase for both 1 & 2 is tëst

        string[] relativeIncludedDirectories1 = new string[] {
            @"TestData\Include1\IncludeSub11",
            @"TestData\Include1\IncludeSub12",
        };
        string[] relativeExcludedDirectories1 = new string[] {
            @"TestData\Exclude1\ExcludeSub11",
            @"TestData\Exclude1\ExcludeSub12",
        };

        _expectedAbsIncludedDirectories1 = relativeIncludedDirectories1.Select(x => Path.GetFullPath(x)).ToArray();
        _expectedAbsExcludedDirectories1 = relativeExcludedDirectories1.Select(x => Path.GetFullPath(x)).ToArray();

        string[] relativeIncludedFiles1 = new string[]
        {
            @"TestData\Include1\IncludeSub11\IncludeText11Lowercase.txt",
            @"TestData\Include1\IncludeSub11\IncludeText11Uppercase.txt",
            @"TestData\Include1\IncludeSub12\IncludeText12Lowercase.txt",
            @"TestData\Include1\IncludeSub12\IncludeText12Uppercase.txt",
        };

        string[] relativeExcludedFiles1 = new string[]
        {
            @"TestData\Exclude1\ExcludeSub11\ExcludeText11Lowercase.txt",
            @"TestData\Exclude1\ExcludeSub11\ExcludeText11Uppercase.txt",
            @"TestData\Exclude1\ExcludeSub12\ExcludeText12Lowercase.txt",
            @"TestData\Exclude1\ExcludeSub12\ExcludeText12Uppercase.txt",
        };

        _expectedAbsIncludedFiles1 = relativeIncludedFiles1.Select(x => Path.GetFullPath(x)).ToArray();
        _expectedAbsExcludedFiles1 = relativeExcludedFiles1.Select(x => Path.GetFullPath(x)).ToArray();


        // takes _expectedAbsIncludedDirectories and _expectedAbsExcludedDirectories, adds "+" to the beginning of each, concats both arrays and assigns it to string[] configDirectoryLines

        string[] configDirectoryLines1 = _expectedAbsIncludedDirectories1.Select(x => "+" + x).Concat(_expectedAbsExcludedDirectories1.Select(x => "-" + x)).ToArray();

        string[] configFileExtLines1 = new string[] 
        { 
            ">.txt" 
        };

        // combine the absolute paths with the file matcher lines to make the config file
        string[] configLines1 = configDirectoryLines1.Concat(configFileExtLines1).ToArray();

        // alt config can just have a different file matcher
        File.WriteAllLines(@"thsearch.txt", configLines1);

        #endregion

        #region 2 Test files 2

        string[] relativeIncludedDirectories2 = new string[] {
            @"TestData\Include2\IncludeSub21",
            @"TestData\Include2\IncludeSub22",
        };
        string[] relativeExcludedDirectories2 = new string[] {
            @"TestData\Exclude2\ExcludeSub21",
            @"TestData\Exclude2\ExcludeSub22",
        };

        _expectedAbsIncludedDirectories2 = relativeIncludedDirectories2.Select(x => Path.GetFullPath(x)).ToArray();
        _expectedAbsExcludedDirectories2 = relativeExcludedDirectories2.Select(x => Path.GetFullPath(x)).ToArray();

        string[] relativeIncludedFiles2 = new string[]
        {
            @"TestData\Include2\IncludeSub21\IncludeText21Lowercase.txt",
            @"TestData\Include2\IncludeSub21\IncludeText21Uppercase.txt",
            @"TestData\Include2\IncludeSub22\IncludeText22Lowercase.txt",
            @"TestData\Include2\IncludeSub22\IncludeText22Uppercase.txt",
        };

        string[] relativeExcludedFiles2 = new string[]
        {
            @"TestData\Exclude2\ExcludeSub21\ExcludeText21Lowercase.txt",
            @"TestData\Exclude2\ExcludeSub21\ExcludeText21Uppercase.txt",
            @"TestData\Exclude2\ExcludeSub22\ExcludeText22Lowercase.txt",
            @"TestData\Exclude2\ExcludeSub22\ExcludeText22Uppercase.txt",
        };

        _expectedAbsIncludedFiles2 = relativeIncludedFiles2.Select(x => Path.GetFullPath(x)).ToArray();
        _expectedAbsExcludedFiles = relativeExcludedFiles2.Select(x => Path.GetFullPath(x)).ToArray();


        // takes _expectedAbsIncludedDirectories and _expectedAbsExcludedDirectories, adds "+" to the beginning of each, concats both arrays and assigns it to string[] configDirectoryLines

        string[] configDirectoryLines2 = _expectedAbsIncludedDirectories2.Select(x => "+" + x).Concat(_expectedAbsExcludedDirectories2.Select(x => "-" + x)).ToArray();

        string[] configFileExtLines2 = new string[] 
        {
            ">.txt" 
        };

        // combine the absolute paths with the file matcher lines to make the config file
        string[] configLines2 = configDirectoryLines2.Concat(configFileExtLines2).ToArray();

        // alt config can just have a different file matcher
        File.WriteAllLines(@"thsearch2.txt", configLines2);

        #endregion

    }

    [TestMethod]
    public void ConfigFileParser_ShouldHaveCorrectIncludedDirectories1()
    {
        // Arrange
        List<string> testGeneratedIncludedDirs = _expectedAbsIncludedDirectories1.ToList();
        ConfigFileParser config = new ConfigFileParser("thsearch.txt");

        // Act
        List<string> configIncludedDirs = config.IncludedDirectories;

        // Assert
        CollectionAssert.AreEqual(testGeneratedIncludedDirs, configIncludedDirs);
    }
    public void ConfigFileParser_ShouldHaveCorrectIncludedDirectories2()
    {
        // Arrange
        List<string> testGeneratedIncludedDirs = _expectedAbsIncludedDirectories2.ToList();
        ConfigFileParser config = new ConfigFileParser("thsearch2.txt");

        // Act
        List<string> configIncludedDirs = config.IncludedDirectories;

        // Assert
        CollectionAssert.AreEqual(testGeneratedIncludedDirs, configIncludedDirs);
    }



    [TestMethod]
    public void ConfigFileParser_ShouldHaveCorrectExcludedDirectories1()
    {
        // Arrange
        List<string> testGeneratedExcludedDirs = _expectedAbsExcludedDirectories1.ToList();;
        ConfigFileParser config = new ConfigFileParser("thsearch.txt");

        // Act
        List<string> configExcludedDirs = config.ExcludedDirectories;

        // Assert
        CollectionAssert.AreEqual(testGeneratedExcludedDirs, configExcludedDirs);
    }
    public void ConfigFileParser_ShouldHaveCorrectExcludedDirectories2()
    {
        // Arrange
        List<string> testGeneratedExcludedDirs = _expectedAbsExcludedDirectories2.ToList();;
        ConfigFileParser config = new ConfigFileParser("thsearch2.txt");

        // Act
        List<string> configExcludedDirs = config.ExcludedDirectories;

        // Assert
        CollectionAssert.AreEqual(testGeneratedExcludedDirs, configExcludedDirs);
    }



    [TestMethod]
    public void Main_WithIncludedFile_ShouldPrintIncludedFiles1()
    {
        // Arrange
        string searchString = "tëst";
        string configName = "thsearch";

        List<string> expectedOutput = _expectedAbsIncludedFiles1.Select(Path.GetFullPath).ToList();

        // Expected that the main method will add a new line character after the last path. Using split below this gets converted to an empty string at the end of the list. Let's emulate that:
        expectedOutput.Add("");

        // Act
        using (ConsoleOutput consoleOutput = new ConsoleOutput())
        {
            Program.Main(new string[] { searchString, configName });
            List<string> output = consoleOutput.GetOutput().Split(Environment.NewLine).ToList();

            // Assert - Currently not working because threads, unlike expectedOutput can deliver paths in any order. This assertion needs to be order independent
            CollectionAssert.AreEquivalent(expectedOutput, output);
        }
    }
    [TestMethod]
    public void Main_WithIncludedFile_ShouldPrintIncludedFiles2()
    {
        // Arrange
        string searchString = "tëst";
        string configName = "thsearch2";

        List<string> expectedOutput = _expectedAbsIncludedFiles2.Select(Path.GetFullPath).ToList();

        // Expected that the main method will add a new line character after the last path. Using split below this gets converted to an empty string at the end of the list. Let's emulate that:
        expectedOutput.Add("");

        // Act
        using (ConsoleOutput consoleOutput = new ConsoleOutput())
        {
            Program.Main(new string[] { searchString, configName });
            List<string> output = consoleOutput.GetOutput().Split(Environment.NewLine).ToList();

            // Assert - Currently not working because threads, unlike expectedOutput can deliver paths in any order. This assertion needs to be order independent
            CollectionAssert.AreEquivalent(expectedOutput, output);
        }
    }

    [TestMethod]
    public void Main_PerformanceTest()
    {
        // Arrange
        var searchString = "tëst";
        var configName = "thsearch";

        // Start the stopwatch
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        // Act
        Program.Main(new string[] { searchString, configName });

        // Stop the stopwatch
        stopwatch.Stop();

        // Write the elapsed time to the test results
        TestContext.WriteLine($"Elapsed time: {stopwatch.ElapsedMilliseconds}ms");
    }

    [TestMethod]
    public void TestFileContainsString()
    {
        // Arrange
        string file = _expectedAbsIncludedFiles1[0];
        string searchString = "tëst";

        // Act
        bool result = Program.FileContainsString(file, searchString);

        // Assert
        Assert.IsTrue(result);
    }

    // no need for an alt version
    [TestMethod]
    public void TestGetMatchingFiles1()
    {
        // Arrange
        string[] directories = _expectedAbsIncludedDirectories1;
        
        
        List<string> fileExtensions = new List<string> {".txt"};
        List<string> excludedDirs = _expectedAbsExcludedDirectories1.ToList();

        List<string> expectedOutput = _expectedAbsIncludedFiles1.ToList();

        // Act
        // runs Program.GetMatchingFiles for each item of directories
        List<string> output = directories.SelectMany(d => Program.GetMatchingFiles(d, fileExtensions, excludedDirs)).ToList();


        // Assert
        CollectionAssert.AreEquivalent(expectedOutput, output);
    }
}