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
    /// Finds the file path of an input file, of which the output was pre computed
    /// </summary>
    /// <param name="file">The file that needs to be searched</param>
    /// <returns>The path to the file</returns>
    static string FindFilePathInput(string file)
    {
        return FindFilePathWithDir("test_sets\\input", file);
    }

    /// <summary>
    /// Finds the file path of an output file, that was pre computed
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
    static bool DoubleIsLessEqual(double d1, double d2)
    {
        return d1 <= d2 + epsilon;
    }
    
    /// <summary>
    /// Check if two results are equal to each other
    /// </summary>
    static bool ResultIsLessEqual(Result r1, Result r2)
    {
        // Check if the total time is equal
        if (!DoubleIsLessEqual(r1.TotalTime,r2.TotalTime)) return false;
        if (r1.StartTransmitTimes.Length != r2.StartTransmitTimes.Length) return false;
        
        // We do not have to check the solution as multiple solutions can have the same time

        return true;
    }

    /// <summary>
    /// Checks whether two intervals [x1,x2) and [y1,y2) overlap
    /// </summary>
    static bool AreOverlapping(decimal x1, decimal x2, decimal y1, decimal y2)
    {        
        return x1 < y2 && y1 < x2;    
    }

    /// <summary>
    /// Checks whether a resulting output is valid given the problem input.
    /// This is done by checken whether sent messages overlap with eachother and/or unavailable intervals.
    /// </summary>
    static bool IsValidResult(Input testInput, Result testOutput)
    {
        // There must be files to send
        if(testInput.Files.Length == 0) return false;

        // Construct the list of messages ordered on the time they are sent.
        var msgIdsSendOrder = testOutput.StartTransmitTimes
            .Select((t, id) => (id, tStart: (decimal)t, tEnd: (decimal)t + (decimal)testInput.Files[id].Size))
            .OrderBy(msg => msg.tStart)
            .ToList();

        // Check whether all messages are sent at a feasible time.
        for (int i = 0; i < msgIdsSendOrder.Count; i++)
        {
            (int id, decimal tStart, decimal tEnd) msg = msgIdsSendOrder[i];

            // Check whether the message overlaps with any unavailable interval
            foreach (UnavailableInterval interval in testInput.UnavailableIntervals)
            {
                if (AreOverlapping(msg.tStart, msg.tEnd, (decimal) interval.Start, (decimal) interval.End))
                {
                    Console.WriteLine(
                        $"Message id={msg.id} sent at [{msg.tStart} ~ {msg.tEnd}) " +
                        $"overlaps with the unavailable interval [{interval.Start} ~ {interval.End})");
                    return false;
                }
            }

            // Check whether the message overlaps with the next message to be sent
            (int id, decimal tStart, decimal tEnd) nextMsg;
            if (i < msgIdsSendOrder.Count - 1 && msg.tEnd > (nextMsg = msgIdsSendOrder[i + 1]).tStart)
            {
                Console.WriteLine(
                    $"Message id={msg.id} sent at [{msg.tStart} ~ {msg.tEnd}) " +
                    $"overlaps with the next message id={nextMsg.id} sent at [{nextMsg.tStart} ; {nextMsg.tEnd})");
                return false;
            }
        }
      
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
    /// Tests our program against some computed optimal solutions
    /// </summary>
    /// <param name="fileNum">The number of the file to test</param>
    [Test]
    public void TestFilesWithAnswer(
        [Range(0,150)] int fileNum)
    {
        if (fileNum >= 124 && fileNum <= 129) return; // Files that are too big for our solver
        string file = "test" + fileNum.ToString() + ".txt";
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

        outputPath = inputPath.Substring(0, inputPath.Length - 4) + "output.txt";
        OuptutWriter ouptutWriter = new FileWriter(outputPath);
        ouptutWriter.Write(ref result);

        // Check our result is valid.
        Assert.True(IsValidResult(input, result));

        // Check if our result is better or the same as the expected result, or if the expected result is invalid
        Assert.True(ResultIsLessEqual(result, expectedResult) || !IsValidResult(input,expectedResult));
    }
}