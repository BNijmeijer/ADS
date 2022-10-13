using ADS;
using File = System.IO.File;

namespace ADSTest;
public class SolverTests
{
    static string FindFilePath(string file)
    {
        if (File.Exists(file)) return file;
            
        string workingDirectory = Environment.CurrentDirectory;
        string testcaseDirectory = workingDirectory + "\\TestCases\\";
        if (Directory.Exists(testcaseDirectory) && File.Exists(testcaseDirectory + file))
        {
            return testcaseDirectory + file;
        }
            
        string projectDirectory = Directory.GetParent(workingDirectory).Parent.Parent.FullName;
        testcaseDirectory = projectDirectory + "\\TestCases\\";
        if (Directory.Exists(testcaseDirectory) && File.Exists(testcaseDirectory + file))
        {
            return testcaseDirectory + file;
        }

        string solutionDirectory = Directory.GetParent(projectDirectory).FullName;
        testcaseDirectory = solutionDirectory + "\\TestCases\\";
        if (Directory.Exists(testcaseDirectory) && File.Exists(testcaseDirectory + file))
        {
            return testcaseDirectory + file;
        }

        return file;
    }

    static Input GetInput(IInputReader inputReader)
    {
        string[] inputLines = inputReader.ReadInput();
        return InputParser.Parse(inputLines);
    }
        
    [Test]
    public void TestFiles(
        [Values("test1.txt", "test2.txt", "test3.txt", "test4.txt", "test5.txt")]
        string file)
    {
        string filepath = FindFilePath(file);
        Input input = GetInput(new FileInputReader(filepath));

        ScheduleSolver solver = new FedorSolver();
        Result result = solver.Solve(input);
        Program.PrintResult(ref result);
    }
}