using System.Globalization;
using ADS;
using File = System.IO.File;

namespace ADSTest;

public class SolverTests
{
    /// <summary>
    /// Find a file in a directory
    /// </summary>
    /// <param name="directory">The directory to search in</param>
    /// <param name="file">The file to look for</param>
    /// <returns>The path to the file</returns>
    static string FindFilePathWithDir(string directory, string file)
    {
        // Check from the root of the computer
        if (File.Exists(file)) return file;

        // Check in the current environment directory
        string workingDirectory = Environment.CurrentDirectory;
        string testcaseDirectory = workingDirectory + "\\" + directory + "\\";
        if (Directory.Exists(testcaseDirectory) && File.Exists(testcaseDirectory + file))
        {
            return testcaseDirectory + file;
        }

        // Check in the current project directory
        string projectDirectory = Directory.GetParent(workingDirectory).Parent.Parent.FullName;
        testcaseDirectory = projectDirectory + "\\" + directory + "\\";
        if (Directory.Exists(testcaseDirectory) && File.Exists(testcaseDirectory + file))
        {
            return testcaseDirectory + file;
        }

        // Check in the current solution directory
        string solutionDirectory = Directory.GetParent(projectDirectory).FullName;
        testcaseDirectory = solutionDirectory + "\\" + directory + "\\";
        if (Directory.Exists(testcaseDirectory) && File.Exists(testcaseDirectory + file))
        {
            return testcaseDirectory + file;
        }

        // File was not found, cannot continue if it is not there
        throw new Exception("File was not found");
    }

    /// <summary>
    /// Search a file in the TestCases folder
    /// </summary>
    /// <param name="file">The file that needs to be searched</param>
    /// <returns>The path to the file</returns>
    static string FindFilePathStandard(string file)
    {
        return FindFilePathWithDir("TestCases", file);
    }

    /// <summary>
    /// Finds the file path of an input file, of which the output was hand computed
    /// </summary>
    /// <param name="file">The file that needs to be searched</param>
    /// <returns>The path to the file</returns>
    static string FindFilePathInput(string file)
    {
        return FindFilePathWithDir("test_sets\\input", file);
    }

    /// <summary>
    /// Finds the file path of an output file, that was hand computed
    /// </summary>
    /// <param name="file">The file that needs to be searched</param>
    /// <returns>The path to the file</returns>
    static string FindFilePathOutput(string file)
    {
        return FindFilePathWithDir("test_sets\\output", file);
    }

    /// <summary>
    /// Retrieves the input from an input reader
    /// </summary>
    static Input GetInput(IInputReader inputReader)
    {
        string[] inputLines = inputReader.ReadInput();
        return InputParser.Parse(inputLines);
    }

    /// <summary>
    /// Small epsilon for double checking
    /// </summary>
    private const double epsilon = 0.0001;

    /// <summary>
    /// Check if two doubles are equal, keeping in mind that doubles might have
    /// precision problems
    /// </summary>
    static bool DoubleIsEqual(double d1, double d2)
    {
        return d1 - epsilon <= d2 && d1 + epsilon >= d2;
    }
    
    /// <summary>
    /// Check if two results are equal to each other
    /// </summary>
    static bool ResultIsEqual(Result r1, Result r2)
    {
        // Check if the total time is equal
        if (!DoubleIsEqual(r1.TotalTime,r2.TotalTime)) return false;
        if (r1.StartTransmitTimes.Length != r2.StartTransmitTimes.Length) return false;
        
        // We do not have to check the solution as multiple solutions can have the same time

        return true;
    }
        
    /// <summary>
    /// Does not test, but runs for all of our testcases and writes the output to a file.
    /// This is for ease of use only, such that we can press the 'run all tests' button
    /// to test for all of our test cases.
    /// </summary>
    /// <param name="file">The file to test for</param>
    [Test]
    public void TestFiles(
        [Values("test1.txt", "test2.txt", "test3.txt", "test4.txt", "test5.txt")]
        string file)
    {
        string filepath = FindFilePathStandard(file);
        Input input = GetInput(new FileInputReader(filepath));

        ScheduleSolver solver = new FedorSolver();
        Result result = solver.Solve(input);

        string outputPath = filepath.Substring(0, filepath.Length - 4) + "output.txt";
        OuptutWriter ouptutWriter = new FileWriter(outputPath);
        ouptutWriter.Write(ref result);
    }

    /// <summary>
    /// Tests our program against some hand computed optimal solutions
    /// </summary>
    /// <param name="file">The file to test</param>
    [Test]
    public void TestFilesWithAnser(
        [Values("test1.txt", "test2.txt")] 
        string file)
    {
        string inputPath = FindFilePathInput(file);
        Input input = GetInput(new FileInputReader(inputPath));

        ScheduleSolver solver = new FedorSolver();
        Result result = solver.Solve(input);

        string outputPath = FindFilePathOutput(file);
        Result expectedResult = new Result();

        string[] lines = File.ReadAllLines(outputPath);
        expectedResult.TotalTime = double.Parse(lines[0], CultureInfo.InvariantCulture);

        expectedResult.StartTransmitTimes = new double[lines.Length - 1];
        for (int i = 1; i < lines.Length; i++)
        {
            expectedResult.StartTransmitTimes[i - 1] = double.Parse(lines[i], CultureInfo.InvariantCulture);
        }
        
        Assert.True(ResultIsEqual(result,expectedResult));
    }
}