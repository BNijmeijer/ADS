using System;
namespace ADS;
public class Program
{
    static void Main(string[] args)
    {
        IInputReader reader = new ConsoleInputReader();
        string[] inputLines = reader.ReadInput();
        Input input = InputParser.Parse(inputLines);
    }
}