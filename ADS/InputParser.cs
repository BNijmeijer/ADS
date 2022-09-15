namespace ADS;

/// <summary>
/// The Files that need to be transported
/// </summary>
public struct File
{
    public double Size;
}

/// <summary>
/// The unavailable intervals that can happen on the timeline
/// </summary>
public struct UnavailableInterval
{
    public double Start;
    public double Duration;
}

/// <summary>
/// Contains all data needed for a program
/// </summary>
public struct Input
{
    public File[] Files;
    public UnavailableInterval[] UnavailableIntervals;
}

/// <summary>
/// Parses the input lines of the program
/// </summary>
public class InputParser
{
    /// <summary>
    /// Parses the input lines of the program and generates an input struct containing all of the needed information
    /// </summary>
    /// <param name="input">The input lines</param>
    /// <returns>The Input data for the program</returns>
    public static Input Parse(string[] input)
    {
        int line = 0;
        int n = int.Parse(input[line++]);
        File[] files = new File[n];
        for (int i = 0; i < n; i++)
        {
            files[i] = new File();
            files[i].Size = double.Parse(input[line++]);
        }

        int m = int.Parse(input[line++]);
        UnavailableInterval[] intervals = new UnavailableInterval[m];
        for (int i = 0; i < m; i++)
        {
            string[] data = input[line++].Split(',');
            intervals[i] = new UnavailableInterval();
            intervals[i].Start = double.Parse(data[0]);
            intervals[i].Duration = double.Parse(data[1]);
        }

        intervals = intervals.OrderBy(x => x.Start).ToArray();
        
        Input result = new Input();
        result.Files = files;
        result.UnavailableIntervals = intervals;
        return result;
    }
}