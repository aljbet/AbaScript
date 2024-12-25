﻿using AbaScript;
using AbaScript.AntlrClasses;
using Antlr4.Runtime;
using FluentAssertions;

namespace LexerAndParserTesting;

[TestFixture]
public class VariablesVisitorOutputTests
{
    [Test]
    public void ShouldPrintAssignedVariable()
    {
        const string code = @" int x = 5; print(x); ";

        var parser = CreateParser(code);
        var tree = parser.script();

        parser.NumberOfSyntaxErrors.Should().Be(0);

        using var sw = new StringWriter();
        Console.SetOut(sw);

        var visitor = new AbaScriptCustomVisitor();
        visitor.Visit(tree);

        var output = sw.ToString().Trim();
        output.Should().Be("5");
    }

    [Test]
    public void ShouldThrowOnInvalidVariableAssignment()
    {
        const string code = @"
            int x = 3;
            x = ""notANumber"";
        ";

        var parser = CreateParser(code);
        var tree = parser.script();

        parser.NumberOfSyntaxErrors.Should().Be(0, "the grammar itself might not flag this as a syntax error");

        var visitor = new AbaScriptCustomVisitor();
        FluentActions.Invoking(() => visitor.Visit(tree))
            .Should().Throw<InvalidOperationException>("assigning a string to an int variable should fail");
    }

    private AbaScriptParser CreateParser(string input)
    {
        var str = new AntlrInputStream(input);
        var lexer = new AbaScriptLexer(str);
        var tokens = new CommonTokenStream(lexer);
        var p = new AbaScriptParser(tokens);

        var errorListener = new AbaScriptCustomListener();
        lexer.RemoveErrorListeners();
        lexer.AddErrorListener(errorListener);
        p.RemoveErrorListeners();
        p.AddErrorListener(errorListener);

        return p;
    }
}
