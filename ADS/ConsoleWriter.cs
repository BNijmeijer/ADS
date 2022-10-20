using System.Globalization;

namespace ADS;

public class ConsoleWriter : OuptutWriter
{
    public override void Write(ref Result result)
    {
        Console.WriteLine(result.TotalTime.ToString(CultureInfo.InvariantCulture));
        for (int i = 0; i < result.StartTransmitTimes.Length; i++)
        {
            Console.WriteLine(result.StartTransmitTimes[i].ToString(CultureInfo.InvariantCulture));
        }
    }
}