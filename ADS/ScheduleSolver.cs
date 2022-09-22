namespace ADS;

/// <summary>
/// The result struct of the program
/// </summary>
public struct Result
{
    public double TotalTime;
    public double[] StartTransmitTimes;
}

/// <summary>
/// A wrapper for a solver of the problem
/// </summary>
public abstract class ScheduleSolver
{
    /// <summary>
    /// Sets up the solver by creating the result
    /// </summary>
    /// <param name="input">The input of the solver</param>
    /// <returns>The result</returns>
    public Result Solve(Input input)
    {
        Result result = new Result();
        result.TotalTime = 0;
        result.StartTransmitTimes = new double[input.Files.Length];
        SolveInput(input, ref result);
        return result;
    }
    
    /// <summary>
    /// Should be overriden and should solve the input and store the result in the result variable
    /// </summary>
    /// <param name="input">The input</param>
    /// <param name="result">The result</param>
    protected abstract void SolveInput(Input input, ref Result result);
}