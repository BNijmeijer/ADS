namespace ADS;

/// <summary>
/// Read input from a file
/// </summary>
public class FileInputReader : IInputReader
{
    /// <summary>
    /// The path of the file to read
    /// </summary>
    private readonly string _path;
    
    /// <summary>
    /// Constructs the file input reader by setting the path to the file
    /// </summary>
    /// <param name="path">The path to the input file</param>
    public FileInputReader(string path)
    {
        this._path = path;
    }
    
    /// <summary>
    /// Reads the input file and returns all lines of the file such that the program can use the lines
    /// </summary>
    /// <returns>The input of the program</returns>
    public string[] ReadInput()
    {
        return System.IO.File.ReadAllLines(_path);
    }
}