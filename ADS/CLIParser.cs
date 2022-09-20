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
        RootCommand rootCommand = new RootCommand("Computes the optimal schedule for the Telescope Scheduling Problem");

        Option<bool> online =
            new Option<bool>("--online", "Whether the algorithm should run in online or offline mode");
        rootCommand.AddOption(online);

        Option<bool> console =
            new Option<bool>("--console", "Whether the program should use the console as input");

        Option<string> files =
            new Option<string>("--files", "What files should be read, separate files with a ';'. Ignored when --console is specified and does not work with --file");
        rootCommand.AddOption(files);

        rootCommand.SetHandler((onlineValue, consoleValue, filesValue) =>
        {
            // need to specify an input type
            if (filesValue == null && !consoleValue)
            {
                Console.WriteLine("You need to specify --console, --file or --files");
                _cliInput.Succes = false;
                return;
            }

            // set values
            _cliInput.Succes = true;
            _cliInput.Online = onlineValue;
            _cliInput.UseConsole = consoleValue;
            
            // don't continue if consoleValue is specified, as --files will be ignored
            if (consoleValue) return;
            
            _cliInput.Files = filesValue.Split(';');

        },online, console, files);

        rootCommand.Invoke(args);
        return _cliInput;
    }
}