using Gurobi; // I don't know whether this works for you immediately, probably you have to download Gurobi as well.

namespace ADS;
public class FedorSolver : ScheduleSolver
{
    protected GRBEnv env;
    protected GRBModel model;

    GRBVar[] y;
    GRBVar[,] x;
    double[] T;// These are the interval lengths.
    double[] Tstart;
    double[] s;// These are the sizes of the images.
    int m;
    int n;

    int L;

    /// <summary>
    /// Initialising the Gurobi Solver
    /// </summary>
    public FedorSolver()
    {
        env = new();
        env.Set("LogFile", "mipi.log");
        env.Start();
        model = new(env);
    }
    protected override void SolveInput(Input input, ref Result result)
    {
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
        double sumsizes = 0;
        for (int i = 0; i < n; i++)
            sumsizes += s[i];
        T[m] = sumsizes;

        // Solve Stage 1:
        Stage1();
        model.Dispose();

        // Continue to Stage 2:
        model = new(env);
        Stage2();


        result = makeResult();
    }
    /// <summary>
    /// Stage 1 of the FedorModel
    /// </summary>
    void Stage1()
    {
        // Now, we create decision variables and add them to the model
        CreateXDecisionVariables();
        CreateYDecisionVariables();

        // Now, we add the constraints to the model
        FirstIntervalsConstraints();
        ImagesAreSentConstraints1();
        ImagesFitInIntervalsConstraints1();

        model.SetObjective(Goal1(), GRB.MINIMIZE);
        model.Optimize();

        L = (int)model.ObjVal; // The objective always is an integer.
    }
    /// <summary>
    /// Stage 2 of the FedorModel
    /// </summary>
    void Stage2()
    {
        CreateXDecisionVariables();

        ImagesAreSentConstraints2();
        ImagesFitInIntervalsConstraints2();

        model.SetObjective(Goal2(), GRB.MINIMIZE);
        model.Optimize();
    }
    /// <summary>
    /// Makes the result object after the optimization step
    /// </summary>
    /// <returns></returns>
    Result makeResult()
    {
        Result result = new()
        {
            TotalTime = model.ObjVal + Tstart[L - 1],
            StartTransmitTimes = new double[n]
        };
        double[] transmittimes = (double[])Tstart.Clone();


        return result;
    }
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
            goal += x[i, L] * s[i];
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

