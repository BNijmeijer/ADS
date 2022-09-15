namespace ADS;

public struct File
{
    public double Size;
}

public struct UnavailableInterval
{
    public double Start;
    public double Duration;
}

public struct Input
{
    public File[] Files;
    public UnavailableInterval[] UnavailableIntervals;
}

public class InputParser
{
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