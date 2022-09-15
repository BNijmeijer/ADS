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
        IInputReader reader = args.Length > 0 ? new FileInputReader(args[0]) : new ConsoleInputReader();
        string[] inputLines = reader.ReadInput();
        Input input = InputParser.Parse(inputLines);
    }
}