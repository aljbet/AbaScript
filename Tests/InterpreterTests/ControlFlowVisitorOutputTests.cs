using AbaScript;
using AbaScript.AntlrClasses;
using Antlr4.Runtime;
using FluentAssertions;

namespace Tests.InterpreterTests;

[TestFixture]
public class ControlFlowVisitorOutputTests
{
    [Test]
    public void ShouldPrintInIfBlock()
    {
        const string code = @"
            int x = 3;
            if (x < 5) {
                print(""OK"");
            }
        ";

        var parser = CreateParser(code);
        var tree = parser.script();

        parser.NumberOfSyntaxErrors.Should().Be(0);

        using var sw = new StringWriter();
        Console.SetOut(sw);

        var visitor = new AbaScriptCustomVisitor();
        visitor.Visit(tree);

        var output = sw.ToString().Trim();
        output.Should().Be("OK");
    }

    [Test]
    public void ShouldThrowWhenConditionIsNotBoolean()
    {
        const string code = @"
            int x = 3;
            if (x + 2) {
                print(""Invalid"");
            }
        ";

        var parser = CreateParser(code);
        var tree = parser.script();
        parser.NumberOfSyntaxErrors.Should().Be(1);
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