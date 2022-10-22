using System.Globalization;

namespace ADS;

/// <summary>
/// A writer that can print a result to the console
/// </summary>
public class ConsoleWriter : OuptutWriter
{
    /// <summary>
    /// Writes the result to the console
    /// </summary>
    /// <param name="result">The result that needs to be written</param>
    public override void Write(ref Result result)
    {
        Console.WriteLine(result.TotalTime.ToString(CultureInfo.InvariantCulture));
        for (int i = 0; i < result.StartTransmitTimes.Length; i++)
        {
            Console.WriteLine(result.StartTransmitTimes[i].ToString(CultureInfo.InvariantCulture));
        }
    }
}