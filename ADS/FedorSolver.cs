using Gurobi;

namespace ADS;
/// <summary>
/// Solves the offline telescoping problem using the Fedor Method
/// </summary>
public class FedorSolver : ScheduleSolver
{
    //////////////////////////////////////////////////////////////////
    /// Class variables
    //////////////////////////////////////////////////////////////////
    
    private readonly GRBEnv _env;
    
    private GRBModel _model;

    /// <summary>
    /// y[j] denotes if T_j is part of the consecutive sequence
    /// </summary>
    private GRBVar[]? _y;
    
    /// <summary>
    /// x[i,j] denotes if image f_i is send in interval T_j
    /// </summary>
    private GRBVar[,]? _x;
    
    /// <summary>
    /// T[j] is the length of interval T_j
    /// </summary>
    private double[]? _T;
    
    /// <summary>
    /// TStart[j] is the start time of interval T_j
    /// </summary>
    private double[]? _TStart;
    
    /// <summary>
    /// s[i] is the size of image f_i
    /// </summary>
    private double[]? _s;
    
    /// <summary>
    /// The amount of unavailability intervals
    /// </summary>
    private int _m = 0;
    
    /// <summary>
    /// The number of images
    /// </summary>
    private int _n = 0;

    /// <summary>
    /// The objective value of the first optimization phase
    /// </summary>
    private int _L = 0;

    /*----------------------------------------------------------------------------------------*/
    /*                            Main functions of the class                                 */
    /*----------------------------------------------------------------------------------------*/

    /// <summary>
    /// Initialising the Gurobi Solver
    /// </summary>
    public FedorSolver()
    {
        _env = new();
        _env.Set("LogFile", "mipi.log");
        _env.MIPGap = 1.00e-7;
        _env.MIPGapAbs = 1.00e-4;
        _env.Start();
        _model = new(_env);
    }
    
    /// <summary>
    /// Solves the offline telescoping problem
    /// </summary>
    /// <param name="input"></param>
    /// <param name="result"></param>
    protected override void SolveInput(Input input, ref Result result)
    {
        // Initialize the variables 
        _m = input.UnavailableIntervals.Length;
        _n = input.Files.Length;

        // Sort the input unavailable intervals on start time
        Array.Sort(input.UnavailableIntervals);

        _s = new double[_n];
        for (int i = 0; i < _n; i++)
        {
            _s[i] = input.Files[i].Size;
        }

        _TStart = new double[_m + 1];
        _T = new double[_m+1];
        _TStart[0] = 0;
        _T[0] = input.UnavailableIntervals[0].Start;
        for (int j = 1; j < _m; j++)
        {
            _TStart[j] = input.UnavailableIntervals[j - 1].Start + input.UnavailableIntervals[j - 1].Duration;
            _T[j] = input.UnavailableIntervals[j].Start - _TStart[j];
        }
        _TStart[_m] = input.UnavailableIntervals[_m - 1].Start + input.UnavailableIntervals[_m - 1].Duration;

        // Make the last interval 'infinitely large'
        double sumSizes = 0;
        for (int i = 0; i < _n; i++)
            sumSizes += _s[i];
        _T[_m] = sumSizes;

        // Solve Stage 1:
        Stage1();
        _L = (int)_model.ObjVal; // The objective always is an integer.
        _model.Dispose();

        // Solve Stage 2:
        _model = new(_env);
        Stage2();

        // Write out the result
        MakeResult(ref result);

        _model.Dispose();
        _env.Dispose();
    }
    
    /// <summary>
    /// Stage 1 of the FedorModel
    /// </summary>
    void Stage1()
    {
        // Create decision variables and add them to the model
        CreateXDecisionVariables();
        CreateYDecisionVariables();

        // Add the constraints to the model
        FirstIntervalsConstraints();
        ImagesAreSentConstraints1();
        ImagesFitInIntervalsConstraints1();

        // Set the goal, and proceed the optimization
        _model.SetObjective(Goal1(), GRB.MINIMIZE);
        _model.Optimize();
    }
    
    /// <summary>
    /// Stage 2 of the FedorModel
    /// </summary>
    void Stage2()
    {
        // Create decision variables and add them to the model
        CreateXDecisionVariables();

        // Add the constraints to the model
        ImagesAreSentConstraints2();
        ImagesFitInIntervalsConstraints2();

        // Set the goal, and proceed the optimization
        _model.SetObjective(Goal2(), GRB.MINIMIZE);
        _model.Optimize();
    }
    
    /// <summary>
    /// Makes the result object after the optimization step
    /// </summary>
    /// <returns></returns>
    void MakeResult(ref Result result)
    {
        // First we calculate the total time.
        // We should therefore add the start time of the last used interval to the cumulative file size in the last interval.
        result.TotalTime = _model.ObjVal + _TStart[_L - 1];

        // Now, we want to calculate the transmission times.
        // Therefore, we first make an (ugly code) deep code of an array
        double[] transmitTimes = new double[_L];
        for (int j = 0; j < _L; j++)
        {
            transmitTimes[j] = _TStart[j];
        }
        
        // For each image, we give it the first possible transmit time in the assigned time interval.
        for (int i = 0; i < _n; i++)
        {
            for (int j=0; j<_L; j++)
            {
                if (_x[i,j].X >= 0.5)// X should be 0 or 1, but rounding errors occur
                {
                    result.StartTransmitTimes[i] = transmitTimes[j];
                    transmitTimes[j] += _s[i];
                }
            }
        }
    }

    /*----------------------------------------------------------------------------------------*/
    /* The different constraints and goals which are used as building blocks for ILP-problems */
    /*----------------------------------------------------------------------------------------*/
    
    /// <summary>
    /// Sets the goal for Stage 1 of the optimization
    /// </summary>
    /// <returns></returns>
    GRBLinExpr Goal1()
    {
        GRBLinExpr goal = 0;
        for (int j = 0; j < _m + 1; j++)
        {
            goal += _y[j];
        }
        return goal;
    }
    
    /// <summary>
    /// Sets the goal for Stage 2 of the optimzation
    /// </summary>
    /// <returns></returns>
    GRBLinExpr Goal2()
    {
        GRBLinExpr goal = 0;
        for (int i=0; i<_n; i++)
        {
            goal += _x[i, _L-1] * _s[i];
        }
        return goal;
    }
   
    /// <summary>
    /// Creates Y decision variables and add them to the model
    /// </summary>
    void CreateYDecisionVariables()
    {
        _y = new GRBVar[_m + 1];
        for (int j = 0; j < _m + 1; j++)
        {
            _y[j] = _model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, "y_" + j.ToString());
        }
    }
    
    /// <summary>
    /// Creates X decision variables and add them to the model
    /// </summary>
    void CreateXDecisionVariables()
    {
        _x = new GRBVar[_n, _m + 1];
        for (int i = 0; i < _n; i++)
        {
            for (int j = 0; j < _m + 1; j++)
            {
                _x[i, j] = _model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, "y_" + i.ToString());
            }
        }
    }
    
    /// <summary>
    /// y_j \geq y_{j+1}
    /// </summary>
    void FirstIntervalsConstraints()
    {
        for (int j=0; j<_m; j++)
        {
            _model.AddConstr(_y[j] >= _y[j + 1], "y_" + j.ToString() + ">=y_" + (j + 1).ToString());
        }
    }
    
    /// <summary>
    /// \sum_{j=1}^{m+1}x_{ij} = 1
    /// </summary>
    void ImagesAreSentConstraints1()
    {
        for (int i=0; i<_n; i++)
        {
            GRBLinExpr noOfTimesSend = 0;
            for (int j=0; j<_m+1; j++)
            {
                noOfTimesSend += _x[i, j];
            }
            _model.AddConstr(noOfTimesSend == 1, "Image " + i.ToString() + " is send");
        }
    }
    
    /// <summary>
    /// \sum_{j=1}^{L}x_{ij} = 1
    /// </summary>
    void ImagesAreSentConstraints2()
    {
        for (int i = 0; i < _n; i++)
        {
            GRBLinExpr noOfTimesSend = 0;
            for (int j = 0; j < _L; j++)
            {
                noOfTimesSend += _x[i, j];
            }
            _model.AddConstr(noOfTimesSend == 1, "Image " + i.ToString() + " is send");
        }
    }
    
    /// <summary>
    /// \sum_{i=1}^{n}x_{ij}s_i \leq |T_j| y_j
    /// </summary>
    void ImagesFitInIntervalsConstraints1()
    {
        for (int j=0; j<_m+1; j++)
        {
            GRBLinExpr totalSizeSend = 0;
            for(int i=0; i<_n; i++)
            {
                totalSizeSend += _x[i, j] * _s[i];
            }
            _model.AddConstr(totalSizeSend <= _T[j] * _y[j], "Fits in interval " + j.ToString());
        }
    }
    
    /// <summary>
    /// \sum_{i=1}^{n}x_{ij}s_i \leq |T_j|
    /// </summary>
    void ImagesFitInIntervalsConstraints2()
    {
        for (int j = 0; j < _L-1; j++)
        {
            GRBLinExpr totalSizeSend = 0;
            for (int i = 0; i < _n; i++)
            {
                totalSizeSend += _x[i, j] * _s[i];
            }
            _model.AddConstr(totalSizeSend <= _T[j], "Fits in interval " + j.ToString());
        }
    }
}

