using AbaScript.AntlrClasses;
using AbaScript.Helpers;
using AbaScript.InterpreterClasses;
using AbaScript.Services.Interfaces;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace AbaScript.Services;

public class InterpreterService : ICodeRunnable
{
    private AbaScriptInterpreter _interpreter = new();
    private AbaScriptLexerAndParserErrorListener _errorListener = new();
    private AbaScriptMainChecker _mainChecker = new();
    private readonly ICanReadException _exceptionReader = new InterpreterExceptionReader();

    public object? RunCode(string input)
    {
        SetUpService();
        var tree = SetUpParseTree(input);

        if (_errorListener.HasErrors)
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
        _errorListener = new AbaScriptLexerAndParserErrorListener();
        _mainChecker = new AbaScriptMainChecker();
    }

    private IParseTree SetUpParseTree(string input)
    {
        var lexer = new AbaScriptLexer(new AntlrInputStream(input));
        var tokens = new CommonTokenStream(lexer);
        var parser = new AbaScriptParser(tokens);

        lexer.RemoveErrorListeners();
        lexer.AddErrorListener(_errorListener);
        parser.RemoveErrorListeners();
        parser.AddErrorListener(_errorListener);

        return parser.script();
    }
}