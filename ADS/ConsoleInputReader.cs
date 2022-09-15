namespace ADS;

/// <summary>
/// An IInput reader that reads input from the console
/// </summary>
public class ConsoleInputReader : IInputReader
{
    /// <summary>
    /// Reads the right amount of input from the console such that the program can use the lines
    /// </summary>
    /// <returns>The input of the program</returns>
    public string[] ReadInput()
    {
        List<string> lines = new List<string>();
        
        lines.Add(Console.ReadLine());
        int n = int.Parse(lines[0]);
        for (int i = 0; i < n; i++)
        {
            lines.Add(Console.ReadLine());
        }
        lines.Add(Console.ReadLine());
        int m = int.Parse(lines[lines.Count - 1]);
        for (int i = 0; i < m; i++)
        {
            lines.Add(Console.ReadLine());
        }

        return lines.ToArray();
    }
}