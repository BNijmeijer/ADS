﻿using System;

namespace ADS;

public class Program
{
    /// <summary>
    /// Main entry point of the application
    /// </summary>
    /// <param name="args">The commandline arguments</param>
    static void Main(string[] args)
    {
        CLIInput cliInput = CLIParser.Parse(args);
        if (!cliInput.Succes) return;

        ScheduleSolver solver = null;
        
        if (cliInput.UseConsole)
        {
            RunInput(new ConsoleInputReader(), solver, cliInput.Online);
            return;
        }

        for (int i = 0; i < cliInput.Files.Length; i++)
        {
            RunInput(new FileInputReader(cliInput.Files[i]), solver, cliInput.Online);
        }
    }

    /// <summary>
    /// Runs the solver over a given input
    /// </summary>
    /// <param name="reader">The input reader</param>
    /// <param name="solver">The algorithm that solves the problem</param>
    /// <param name="online">Whether the program will run online or offline</param>
    static void RunInput(IInputReader reader, ScheduleSolver solver, bool online)
    {
        string[] inputLines = reader.ReadInput();
        Input input = InputParser.Parse(inputLines);
        Result result = solver.Solve(input);
        PrintResult(ref result);
    }

    /// <summary>
    /// Prints the result to the console
    /// </summary>
    /// <param name="result">The result</param>
    public static void PrintResult(ref Result result)
    {
        Console.WriteLine(result.TotalTime);
        for (int i = 0; i < result.StartTransmitTimes.Length; i++)
        {
            Console.WriteLine(result.StartTransmitTimes[i]);
        }
    }
}