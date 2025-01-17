using AbaScript.AntlrClasses;
using AbaScript.InterpreterClasses;
using Antlr4.Runtime;
using FluentAssertions;

namespace Tests.InterpreterTests;

public abstract class InterpreterTestBase
{
    protected void ExecuteCode(string code, Action<string> assertions)
    {
        var parser = CreateParser(code);
        var tree = parser.script();

        parser.NumberOfSyntaxErrors.Should().Be(0);

        using var sw = new StringWriter();
        Console.SetOut(sw);

        var visitor = new AbaScriptInterpreter();

        try
        {
            visitor.Visit(tree);
            assertions(sw.ToString().Trim());
        }
        catch (Exception ex)
        {
            assertions(ex.Message);
        }
    }

    private static AbaScriptParser CreateParser(string input)
    {
        var str = new AntlrInputStream(input);
        var lexer = new AbaScriptLexer(str);
        var tokens = new CommonTokenStream(lexer);
        var p = new AbaScriptParser(tokens);

        return p;
    }
}