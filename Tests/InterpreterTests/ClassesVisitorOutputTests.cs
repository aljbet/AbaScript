using AbaScript.AntlrClasses;
using AbaScript.InterpreterClasses;
using Antlr4.Runtime;
using FluentAssertions;

namespace Tests.InterpreterTests;

[TestFixture]
public class ClassesVisitorOutputTests
{
    [Test]
    public void ShouldPrintMethodResultFromClass()
    {
        const string code = @"
            class Vector {
                int data = 10;
                func int Multiply(int factor) {
                    return data * factor;
                }
            }

            Vector v = new Vector;
            print(v.Multiply(4));
        ";

        var parser = CreateParser(code);
        var tree = parser.script();

        parser.NumberOfSyntaxErrors.Should().Be(0);

        using var sw = new StringWriter();
        Console.SetOut(sw);

        var visitor = new AbaScriptInterpreter();
        visitor.Visit(tree);

        var output = sw.ToString().Trim();
        output.Should().Be("40");
    }

    [Test]
    public void ShouldThrowWhenAccessingInvalidField()
    {
        const string code = @"
            class Dummy {
                int validField = 1;
            }

            Dummy d = new Dummy;
            d.invalidField = 5;
        ";

        var parser = CreateParser(code);
        var tree = parser.script();

        parser.NumberOfSyntaxErrors.Should().Be(0);

        var visitor = new AbaScriptInterpreter();
        FluentActions.Invoking(() => visitor.Visit(tree))
            .Should().Throw<InvalidOperationException>("assigning to a non-existent field should fail");
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