using AbaScript.AntlrClasses;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace AbaScript.Services.Interfaces;

public abstract class RunService
{
    protected readonly AbaScriptLexerAndParserErrorListener ErrorListener = new();

    public abstract object? RunCode(string code);
    
    protected IParseTree SetUpParseTree(string input)
    {
        var lexer = new AbaScriptLexer(new AntlrInputStream(input));
        var tokens = new CommonTokenStream(lexer);
        var parser = new AbaScriptParser(tokens);

        lexer.RemoveErrorListeners();
        lexer.AddErrorListener(ErrorListener);
        parser.RemoveErrorListeners();
        parser.AddErrorListener(ErrorListener);

        return parser.script();
    }
}