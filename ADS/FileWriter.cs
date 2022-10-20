using System.Globalization;

namespace ADS;

public class FileWriter : OuptutWriter
{
    private string _file;
    public FileWriter(string file)
    {
        _file = file;
    }
    
    /// <summary>
    /// Writes to the file defined by _file
    /// </summary>
    /// <param name="result">The result that needs to be written</param>
    public override void Write(ref Result result)
    {
        StreamWriter sr = new StreamWriter(_file, false);
        sr.WriteLine(result.TotalTime.ToString(CultureInfo.InvariantCulture));
        for (int i = 0; i < result.StartTransmitTimes.Length; i++)
        {
            sr.WriteLine(result.StartTransmitTimes[i].ToString(CultureInfo.InvariantCulture));
        }
        sr.Flush();
    }
}