using AbaScript.AntlrClasses;
using AbaScript.AntlrClasses.Exceptions;
using Antlr4.Runtime;
using FluentAssertions;

namespace Tests.AnalyzerTests;

[TestFixture]
public class TypeCheckerTests
{
    private AbaScriptTypeChecker _typeChecker;

    [SetUp]
    public void Setup()
    {
        _typeChecker = new AbaScriptTypeChecker();
    }

    [Test]
    public void TestFunctionDefinitionAndCall()
    {
        var script = @"
            func int add(int a, int b) {
                return a + b;
            }

            int result = add(3, 4);
        ";

        Assert.DoesNotThrow(() => _typeChecker.Visit(ParseScript(script)));
    }

    [Test]
    public void TestArrayHandling()
    {
        var script = @"
            int numbers[5];
            int first = numbers[0];
        ";

        Assert.DoesNotThrow(() => _typeChecker.Visit(ParseScript(script)));
    }

    [Test]
    public void TestClassDefinitionAndFieldAccess()
    {
        var script = @"
            class Point {
                int x;
                int y;
            }
            Point p = new Point;
            p.x = 10;
        ";

        Assert.DoesNotThrow(() => _typeChecker.Visit(ParseScript(script)));
    }

    [Test]
    public void TestTypeMismatchInAssignment()
    {
        var script = @"
            int a = 5;
            a = ""string"";
        ";

        Assert.Throws<TypeCheckerException>(() => _typeChecker.Visit(ParseScript(script)));
    }

    [Test]
    public void TestUndefinedFunctionCall()
    {
        var script = @"
            int result = undefinedFunction(3, 4);
        ";

        Assert.Throws<TypeCheckerException>(() => _typeChecker.Visit(ParseScript(script)));
    }

    private static AbaScriptParser.ScriptContext ParseScript(string script)
    {
        var inputStream = new AntlrInputStream(script);
        var lexer = new AbaScriptLexer(inputStream);
        var tokenStream = new CommonTokenStream(lexer);
        var parser = new AbaScriptParser(tokenStream);
        var tree = parser.script();
        return tree;
    }
}