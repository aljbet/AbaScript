using AbaScript.AntlrClasses;
using AbaScript.InterpreterClasses;
using Antlr4.Runtime;
using FluentAssertions;

namespace Tests.InterpreterTests;

[TestFixture]
public class ExpressionsVisitorOutputTests
{
    [Test]
    public void ShouldPrintAdditionResult()
    {
        const string code = @"
            int result = 2 + 3;
            print(result);
        ";

        var parser = CreateParser(code);
        var tree = parser.script();

        parser.NumberOfSyntaxErrors.Should().Be(0);

        using var sw = new StringWriter();
        Console.SetOut(sw);

        var visitor = new AbaScriptInterpreter();
        visitor.Visit(tree);

        var output = sw.ToString().Trim();
        output.Should().Be("5");
    }

    [Test]
    public void ShouldThrowOnInvalidExpression()
    {
        const string code = @"
            int x = 2 + ;
        ";

        var parser = CreateParser(code);
        var tree = parser.script();

        parser.NumberOfSyntaxErrors.Should()
            .BeGreaterThan(0, "missing a right operand for '+' should be a syntax error");
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