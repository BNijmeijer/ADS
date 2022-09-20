using System;
namespace ADS;
public class Program
{
    /// <summary>
    /// Main entry point of the application
    /// </summary>
    /// <param name="args">The commandline arguments</param>
    static void Main(string[] args)
    {
        CLIInput cliInput = CLIParser.Parse(args);
        if (!cliInput.Succes) return;
        
        if (cliInput.UseConsole)
        {
            RunInput(new ConsoleInputReader(), cliInput.Online);
            return;
        }

        for (int i = 0; i < cliInput.Files.Length; i++)
        {
            RunInput(new FileInputReader(cliInput.Files[i]), cliInput.Online);
        }
    }

    static void RunInput(IInputReader reader, bool online)
    {
        string[] inputLines = reader.ReadInput();
        Input input = InputParser.Parse(inputLines);
    }
}