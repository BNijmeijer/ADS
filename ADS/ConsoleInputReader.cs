namespace ADS;

public class ConsoleInputReader : IInputReader
{
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