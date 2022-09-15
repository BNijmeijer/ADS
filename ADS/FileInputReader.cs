namespace ADS;

public class FileInputReader : IInputReader
{
    private readonly string _path;
    public FileInputReader(string path)
    {
        this._path = path;
    }
    
    public string[] ReadInput()
    {
        return System.IO.File.ReadAllLines(_path);
    }
}