using System;
namespace ADS;
public class Program
{
    static void Main(string[] args)
    {
        IInputReader reader = args.Length > 0 ? new FileInputReader(args[0]) : new ConsoleInputReader();
        string[] inputLines = reader.ReadInput();
        Input input = InputParser.Parse(inputLines);
    }
}