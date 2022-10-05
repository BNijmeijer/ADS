using Gurobi; // I don't know whether this works for you immediately, probably you have to download Gurobi as well.

namespace ADS;
/// <summary>
/// Solves the offline telescoping problem using the Fedor Method
/// </summary>
public class FedorSolver : ScheduleSolver
{
    //////////////////////////////////////////////////////////////////
    /// Class variables
    //////////////////////////////////////////////////////////////////
    
    protected GRBEnv env;
    protected GRBModel model;

    /// <summary>
    /// y[j] denotes if T_j is part of the consecutive sequence
    /// </summary>
    GRBVar[] y;
    /// <summary>
    /// x[i,j] denotes if image f_i is send in interval T_j
    /// </summary>
    GRBVar[,] x;
    /// <summary>
    /// T[j] is the length of interval T_j
    /// </summary>
    double[] T;
    /// <summary>
    /// Tstart[j] is the start time of interval T_j
    /// </summary>
    double[] Tstart;
    /// <summary>
    /// s[i] is the size of image f_i
    /// </summary>
    double[] s;
    /// <summary>
    /// The amount of unavailibility intervals
    /// </summary>
    int m;
    /// <summary>
    /// The amound of images
    /// </summary>
    int n;

    /// <summary>
    /// The objetive value of the first optimization phase
    /// </summary>
    int L;

    //////////////////////////////////////////////////////////////////
    /// Main functions of the class
    //////////////////////////////////////////////////////////////////

    /// <summary>
    /// Initialising the Gurobi Solver
    /// </summary>
    public FedorSolver()
    {
        env = new();
        env.Set("LogFile", "mipi.log");
        env.Start();
        model = new(env);

        // And now some ugly initialisation because of compiler-complaints
        T = new double[0];
        Tstart = new double[0];
        s = new double[0];
        m = 0;
        n = 0;
        L = 0;

    }
    /// <summary>
    /// Solves the offline telescoping problem
    /// </summary>
    /// <param name="input"></param>
    /// <param name="result"></param>
    protected override void SolveInput(Input input, ref Result result)
    {
        // Initalise the variables 
        m = input.UnavailableIntervals.Length;
        n = input.Files.Length;

        s = new double[n];
        for (int i = 0; i < n; i++)
        {
            s[i] = input.Files[i].Size;
        }

        Tstart = new double[m + 1];
        T = new double[m+1];
        Tstart[0] = 0;
        T[0] = input.UnavailableIntervals[0].Start;
        for (int j = 1; j < m; j++)
        {
            Tstart[j] = input.UnavailableIntervals[j - 1].Start + input.UnavailableIntervals[j - 1].Duration;
            T[j] = input.UnavailableIntervals[j].Start - Tstart[j];
        }
        Tstart[m] = input.UnavailableIntervals[m - 1].Start + input.UnavailableIntervals[m - 1].Duration;

        // Make the last interval 'infinitely large'
        double sumsizes = 0;
        for (int i = 0; i < n; i++)
            sumsizes += s[i];
        T[m] = sumsizes;

        // Solve Stage 1:
        Stage1();
        L = (int)model.ObjVal; // The objective always is an integer.
        model.Dispose();

        // Continue to Stage 2:
        model = new(env);
        Stage2();

        // Write out the result
        result = makeResult();

        model.Dispose();
        env.Dispose();
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
        model.SetObjective(Goal1(), GRB.MINIMIZE);
        model.Optimize();
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
        model.SetObjective(Goal2(), GRB.MINIMIZE);
        model.Optimize();
    }
    /// <summary>
    /// Makes the result object after the optimization step
    /// </summary>
    /// <returns></returns>
    Result makeResult()
    {
        Result result = new();

        // First we calcluate the total time.
        // We should therefore add the start time of the last used interval to the cumulative file size in the last interval.
        result.TotalTime = model.ObjVal + Tstart[L - 1];

        // Now, we want to calculate the transmission times.
        // Therefore, we first make an (ugly code) deep code of an array
        double[] transmittimes = new double[L];
        for (int j = 0; j < L; j++)
        {
            transmittimes[j] = Tstart[j];
        }
        // For each image, we give it the first possible transmit time in the assigned time interval.
        result.StartTransmitTimes = new double[n];
        for (int i = 0; i < n; i++)
        {
            for (int j=0; j<L; j++)
            {
                if (x[i,j].X == 1)
                {
                    result.StartTransmitTimes[i] = transmittimes[j];
                    transmittimes[j] += s[i];
                }
            }
        }
        return result;
    }

    //////////////////////////////////////////////////////////////////
    /// The different constraints and goals which are used as building blocks for ILP-problems
    //////////////////////////////////////////////////////////////////
    
    /// <summary>
    /// Sets the goal for Stage 1 of the optimization
    /// </summary>
    /// <returns></returns>
    GRBLinExpr Goal1()
    {
        GRBLinExpr goal = 0;
        for (int j = 0; j < m + 1; j++)
        {
            goal += y[j];
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
        for (int i=0; i<n; i++)
        {
            goal += x[i, L-1] * s[i];
        }
        return goal;
    }
    /// <summary>
    /// Creates Y decision variables and add them to the model
    /// </summary>
    void CreateYDecisionVariables()
    {
        y = new GRBVar[m + 1];
        for (int j = 0; j < m + 1; j++)
        {
            y[j] = model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, "y_" + j.ToString());
        }

    }
    /// <summary>
    /// Creates X decision variables and add them to the model
    /// </summary>
    void CreateXDecisionVariables()
    {
        x = new GRBVar[n, m + 1];
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < m + 1; j++)
            {
                x[i, j] = model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, "y_" + i.ToString());
            }
        }
    }
    /// <summary>
    /// y_j \geq y_{j+1}
    /// </summary>
    void FirstIntervalsConstraints()
    {
        for (int j=0; j<m; j++)
        {
            model.AddConstr(y[j] >= y[j + 1], "y_" + j.ToString() + ">=y_" + (j + 1).ToString());
        }
    }
    /// <summary>
    /// \sum_{j=1}^{m+1}x_{ij} = 1
    /// </summary>
    void ImagesAreSentConstraints1()
    {
        for (int i=0; i<n; i++)
        {
            GRBLinExpr noOfTimesSend = 0;
            for (int j=0; j<m+1; j++)
            {
                noOfTimesSend += x[i, j];
            }
            model.AddConstr(noOfTimesSend == 1, "Image " + i.ToString() + " is send");
        }
    }
    /// <summary>
    /// \sum_{j=1}^{L}x_{ij} = 1
    /// </summary>
    void ImagesAreSentConstraints2()
    {
        for (int i = 0; i < n; i++)
        {
            GRBLinExpr noOfTimesSend = 0;
            for (int j = 0; j < L; j++)
            {
                noOfTimesSend += x[i, j];
            }
            model.AddConstr(noOfTimesSend == 1, "Image " + i.ToString() + " is send");
        }
    }
    /// <summary>
    /// \sum_{i=1}^{n}x_{ij}s_i \leq |T_j| y_j
    /// </summary>
    void ImagesFitInIntervalsConstraints1()
    {
        for (int j=0; j<m+1; j++)
        {
            GRBLinExpr totalSizeSend = 0;
            for(int i=0; i<n; i++)
            {
                totalSizeSend += x[i, j] * s[i];
            }
            model.AddConstr(totalSizeSend <= T[j] * y[j], "Fits in interval " + j.ToString());
        }
    }
    /// <summary>
    /// \sum_{i=1}^{n}x_{ij}s_i \leq |T_j|
    /// </summary>
    void ImagesFitInIntervalsConstraints2()
    {
        for (int j = 0; j < L-1; j++)
        {
            GRBLinExpr totalSizeSend = 0;
            for (int i = 0; i < n; i++)
            {
                totalSizeSend += x[i, j] * s[i];
            }
            model.AddConstr(totalSizeSend <= T[j], "Fits in interval " + j.ToString());
        }
    }

}

