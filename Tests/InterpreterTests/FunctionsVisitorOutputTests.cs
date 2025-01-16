using AbaScript.AntlrClasses;
using AbaScript.InterpreterClasses;
using Antlr4.Runtime;
using FluentAssertions;

namespace Tests.InterpreterTests;

[TestFixture]
public class FunctionsVisitorOutputTests
{
    [Test]
    public void ShouldPrintReturnValueFromFunction()
    {
        const string code = @"
            func int GetNumber() {
                return 42;
            }
            print(GetNumber());
        ";

        var parser = CreateParser(code);
        var tree = parser.script();

        parser.NumberOfSyntaxErrors.Should().Be(0);

        using var sw = new StringWriter();
        Console.SetOut(sw);

        var visitor = new AbaScriptInterpreter();
        visitor.Visit(tree);

        var output = sw.ToString().Trim();
        output.Should().Be("42");
    }

    [Test]
    public void ShouldThrowWhenFunctionArgumentCountIsWrong()
    {
        const string code = @"
            func int Sum(int a, int b) {
                return a + b;
            }
            print(Sum(5));
        ";

        var parser = CreateParser(code);
        var tree = parser.script();

        parser.NumberOfSyntaxErrors.Should().Be(0);

        var visitor = new AbaScriptInterpreter();
        FluentActions.Invoking(() => visitor.Visit(tree))
            .Should().Throw<InvalidOperationException>("wrong argument count for function call should fail");
    }

    private AbaScriptParser CreateParser(string input)
    {
        var str = new AntlrInputStream(input);
        var lexer = new AbaScriptLexer(str);
        var tokens = new CommonTokenStream(lexer);
        var p = new AbaScriptParser(tokens);

        return p;
    }
}