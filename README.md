# ADS
This is a project for the master course INFOMADS at Utrecht University about telescope scheduling.

## Project layout
The project contains a few folders:
- `ADS`: this folder contains the core logic for our algorithm. The actual ILP solver is written here, as well as some IO-handling and input parsing.
- `ADSTest`: this folder contains the testing project, used to run test cases with our algorithm.
- `GivenTestCases`: this folder contains all the testcases that were uploaded to the MSTeams by all other groups. These testcases have the input directly followed by the corresponding output.
- `Gurboi`: this folder contains the Gurobi .dll's, used by the Gurobi ILP solver.
- `TestCaseSplitter`: this is a small project to split the testcases in GivenTestCases into input and output files for testing purposes.
- `test_sets`: this folder contains all the test sets split by the `TestCaseSplitter`, these tests are run in `ADSTest`.

## How to run the solver
To run the program you first of all need a Gurobi license. 
After you have added this, you can open any IDE that is able to run NUnit tests, for example Visual Studio Community or .NET Rider.
Run the `TestFilesWithAnswer` test, which runs all the tests in `test_sets` through the algorithm. 
We verify the correctness of the test sets by checking whether the output is valid for the given input. 
We found that some of the tests by other groups (from `GivenTestCases`) were invalid, these cases are logged in the console.
Every test set 'passes' if the our algorithms output matches the test sets 'given' output, or if we find that the test set is invalid.

