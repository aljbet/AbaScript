using AbaScript.AntlrClasses;
using AbaScript.Helpers;
using AbaScript.InterpreterClasses;
using AbaScript.Services.Interfaces;

namespace AbaScript.Services;

public class InterpreterService : RunService
{
    private AbaScriptInterpreter _interpreter = new();
    private AbaScriptMainChecker _mainChecker = new();
    private readonly ICanReadException _exceptionReader = new InterpreterExceptionReader();

    public override object? RunCode(string input)
    {
        SetUpService();
        var tree = SetUpParseTree(input);

        if (ErrorListener.HasErrors)
        {
            Console.WriteLine("Errors detected. Stopping execution.");
            return null;
        }

        _mainChecker.Visit(tree);
        if (!_mainChecker.HasMainFunction())
        {
            Console.WriteLine("Main function wasn't detected. Stopping execution.");
            return null;
        }

        if (_mainChecker.GetGlobalVariables().Count != 0)
        {
            Console.WriteLine("Global variables were detected. Stopping execution.");
            return null;
        }

        try
        {
            return _interpreter.Visit(tree);
        }
        catch (Exception ex)
        {
            _exceptionReader.HandleException(ex);
        }

        return null;
    }

    private void SetUpService()
    {
        _interpreter = new AbaScriptInterpreter();
        _mainChecker = new AbaScriptMainChecker();
    }
}