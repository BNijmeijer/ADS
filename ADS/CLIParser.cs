using System.CommandLine;
using System.CommandLine.Invocation;

namespace ADS;

/// <summary>
/// Different arguments that can be set by the commandline
/// </summary>
public struct CLIInput
{
    public bool Succes;
    public bool Online;
    public bool UseConsole;
    public string[] Files;
}

public class CLIParser
{
    private static CLIInput _cliInput = new CLIInput();
    
    /// <summary>
    /// Parses commandline arguments
    /// </summary>
    /// <param name="args">The commandline arguments</param>
    /// <returns>The output of the commandline</returns>
    public static CLIInput Parse(string[] args)
    {
        RootCommand rootCommand = new RootCommand("Computes the optimal schedule");

        Option<bool> online =
            new Option<bool>("--online", "Whether the algorithm should run in online or offline mode");
        rootCommand.AddOption(online);

        Option<bool> console =
            new Option<bool>("--console", "Whether the program should use the console as input");
        
        Option<string> file =
            new Option<string>("--file", "What file should be read, ignored when --console is specified");
        rootCommand.AddOption(file);
        
        Option<string> files =
            new Option<string>("--files", "What files (multiple) should be read, separate files with a ';'. Ignored when --console is specified and does not work with --file");
        rootCommand.AddOption(files);

        rootCommand.SetHandler((onlineValue, consoleValue, fileValue, filesValue) =>
        {
            // need to specify an input type
            if (fileValue == null && filesValue == null && !consoleValue)
            {
                Console.WriteLine("You need to specify --console, --file or --files");
                _cliInput.Succes = false;
                return;
            }
            
            // cannot specify both --file and --files, unless --console is specified
            if (fileValue != null && filesValue != null && !consoleValue)
            {
                Console.WriteLine("Cannot specifiy --file and --files at the same time");
                _cliInput.Succes = false;
                return;
            }
            
            // set values
            _cliInput.Succes = true;
            _cliInput.Online = onlineValue;
            _cliInput.UseConsole = consoleValue;
            
            // don't continue if consoleValue is specified, as --file and --files will be ignored
            if (consoleValue) return;
            
            // If --files exists we do not need to check --file
            if (filesValue != null)
            {
                _cliInput.Files = filesValue.Split(';');
                return;
            }

            // set --file
            _cliInput.Files = new string[1] { fileValue };

        },online, console, file, files);

        rootCommand.Invoke(args);
        return _cliInput;
    }
}