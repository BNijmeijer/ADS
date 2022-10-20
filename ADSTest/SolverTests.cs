using System.Globalization;
using ADS;
using File = System.IO.File;

namespace ADSTest;

public class SolverTests
{
    static string FindFilePathWithDir(string directory, string file)
    {
        if (File.Exists(file)) return file;

        string workingDirectory = Environment.CurrentDirectory;
        string testcaseDirectory = workingDirectory + "\\" + directory + "\\";
        if (Directory.Exists(testcaseDirectory) && File.Exists(testcaseDirectory + file))
        {
            return testcaseDirectory + file;
        }

        string projectDirectory = Directory.GetParent(workingDirectory).Parent.Parent.FullName;
        testcaseDirectory = projectDirectory + "\\" + directory + "\\";
        if (Directory.Exists(testcaseDirectory) && File.Exists(testcaseDirectory + file))
        {
            return testcaseDirectory + file;
        }

        string solutionDirectory = Directory.GetParent(projectDirectory).FullName;
        testcaseDirectory = solutionDirectory + "\\" + directory + "\\";
        if (Directory.Exists(testcaseDirectory) && File.Exists(testcaseDirectory + file))
        {
            return testcaseDirectory + file;
        }

        return file;
    }

    static string FindFilePathStandard(string file)
    {
        return FindFilePathWithDir("TestCases", file);
    }

    static string FindFilePathInput(string file)
    {
        return FindFilePathWithDir("test_sets\\input", file);
    }

    static string FindFilePathOutput(string file)
    {
        return FindFilePathWithDir("test_sets\\output", file);
    }

    static Input GetInput(IInputReader inputReader)
    {
        string[] inputLines = inputReader.ReadInput();
        return InputParser.Parse(inputLines);
    }

    private const double epsilon = 0.0001;

    static bool DoubleIsEqual(double d1, double d2)
    {
        return d1 - epsilon <= d2 && d1 + epsilon >= d2;
    }
    static bool ResultIsEqual(Result r1, Result r2)
    {
        // Check if the total time is equal
        if (!DoubleIsEqual(r1.TotalTime,r2.TotalTime)) return false;
        if (r1.StartTransmitTimes.Length != r2.StartTransmitTimes.Length) return false;
        
        // We do not have to check the solution as multiple solutions can have the same time

        return true;
    }
        
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