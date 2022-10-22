// See https://aka.ms/new-console-template for more information

using ADS;
using File = System.IO.File;

string GetDir(string directory)
{
    // Check in the current environment directory
    string workingDirectory = Environment.CurrentDirectory;
    string testcaseDirectory = workingDirectory + "\\" + directory + "\\";
    if (Directory.Exists(testcaseDirectory))
    {
        return testcaseDirectory;
    }

    // Check in the current project directory
    string projectDirectory = Directory.GetParent(workingDirectory).Parent.Parent.FullName;
    testcaseDirectory = projectDirectory + "\\" + directory + "\\";
    if (Directory.Exists(testcaseDirectory))
    {
        return testcaseDirectory;
    }

    // Check in the current solution directory
    string solutionDirectory = Directory.GetParent(projectDirectory).FullName;
    testcaseDirectory = solutionDirectory + "\\" + directory + "\\";
    if (Directory.Exists(testcaseDirectory))
    {
        return testcaseDirectory;
    }

    throw new Exception("Could not find GivenTestCases");
}

string GetGivenDir()
{
    return GetDir("GivenTestCases");
}

string GetTestCaseDir()
{
    return GetDir("test_sets");
}

string dir = GetGivenDir();
string[] files = Directory.GetFiles(dir);
for (int i = 0; i < files.Length; i++)
{
    FileInputReader reader = new FileInputReader(files[i]);
    string[] testCase = reader.ReadInput();
    int n = int.Parse(testCase[0]);
    int m = int.Parse(testCase[n + 1]);
    int inputLineAmount = n + m + 2;
    int outputLineAmount = testCase.Length - inputLineAmount;

    string[] input = new string[inputLineAmount];
    for (int j = 0; j < inputLineAmount; j++)
    {
        input[j] = testCase[j];
    }

    string[] output = new string[outputLineAmount];
    for (int j = 0; j < outputLineAmount; j++)
    {
        output[j] = testCase[inputLineAmount + j];
    }
    
    
    File.WriteAllLines(GetTestCaseDir() + "input\\test" + i.ToString() + ".txt", input);
    File.WriteAllLines(GetTestCaseDir() + "output\\test" + i.ToString() + ".txt", output);

}
