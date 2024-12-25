using AbaScript;
using Antlr4.Runtime;
using FluentAssertions;
using FluentAssertions.Execution;

namespace Tests;

[TestFixture]
public class LexerAndParserTests
{
    private static AbaScriptParser CreateParser(string input)
    {
        var inputStream = new AntlrInputStream(input);
        var lexer = new AbaScriptLexer(inputStream);
        var tokenStream = new CommonTokenStream(lexer);
        return new AbaScriptParser(tokenStream);
    }

    [Test]
    public void ValidScript_ShouldHaveNoSyntaxErrors()
    {
        const string script = """
                              
                                          int x = 10;
                                          func int factorial(int n) {
                                              if (n == 0 || n == 1) {
                                                  return 1;
                                              }
                                              return n * factorial(n - 1);
                                          }
                                          print(factorial(x));
                                      
                              """;

        var parser = CreateParser(script);
        var tree = parser.script();

        using (new AssertionScope())
        {
            parser.NumberOfSyntaxErrors.Should().Be(0, "the script should have no syntax errors");
        }
    }

    [Test]
    public void MissingSemicolon_ShouldHaveSyntaxErrors()
    {
        const string script = """
                              
                                          var x = 10
                                          print(x);
                                      
                              """;

        var parser = CreateParser(script);
        var tree = parser.script();

        using (new AssertionScope())
        {
            parser.NumberOfSyntaxErrors.Should().BeGreaterThan(0, "the script is missing a semicolon");
        }
    }

    [Test]
    public void UnmatchedParentheses_ShouldHaveSyntaxErrors()
    {
        const string script = """
                              
                                          print((x + 5);
                                      
                              """;

        var parser = CreateParser(script);
        var tree = parser.script();

        using (new AssertionScope())
        {
            parser.NumberOfSyntaxErrors.Should().BeGreaterThan(0, "the script has unmatched parentheses");
        }
    }

    [Test]
    public void ValidVariableDeclaration_ShouldHaveNoSyntaxErrors()
    {
        const string script = "int x = 5;";
        var parser = CreateParser(script);
        var tree = parser.script();

        using (new AssertionScope())
        {
            parser.NumberOfSyntaxErrors.Should().Be(0, "variable declaration should be valid");
        }
    }

    [Test]
    public void ValidFunctionDefinition_ShouldHaveNoSyntaxErrors()
    {
        const string script = """
                              
                                          func int add(int a, int b) {
                                              return a + b;
                                          }
                                      
                              """;
        var parser = CreateParser(script);
        var tree = parser.script();

        using (new AssertionScope())
        {
            parser.NumberOfSyntaxErrors.Should().Be(0, "function definition should be valid");
        }
    }

    [Test]
    public void ValidIfElseStatement_ShouldHaveNoSyntaxErrors()
    {
        const string script = """
                              
                                          if (x > 0) {
                                              print(x);
                                          } else {
                                              print(-x);
                                          }
                                      
                              """;
        var parser = CreateParser(script);
        var tree = parser.script();

        using (new AssertionScope())
        {
            parser.NumberOfSyntaxErrors.Should().Be(0, "if-else statement should be valid");
        }
    }

    [Test]
    public void InvalidFunctionCall_MissingParenthesis_ShouldHaveSyntaxErrors()
    {
        const string script = "print(x;";
        var parser = CreateParser(script);
        var tree = parser.script();

        using (new AssertionScope())
        {
            parser.NumberOfSyntaxErrors.Should().BeGreaterThan(0, "missing closing parenthesis in function call");
        }
    }

    [Test]
    public void InvalidAssignment_MissingSemicolon_ShouldHaveSyntaxErrors()
    {
        const string script = "x = 10";
        var parser = CreateParser(script);
        var tree = parser.script();

        using (new AssertionScope())
        {
            parser.NumberOfSyntaxErrors.Should().BeGreaterThan(0, "missing semicolon in assignment");
        }
    }

    [Test]
    public void ValidWhileLoop_ShouldHaveNoSyntaxErrors()
    {
        const string script = """
                              
                                          while (x < 10) {
                                              x = x + 1;
                                          }
                                      
                              """;
        var parser = CreateParser(script);
        var tree = parser.script();

        using (new AssertionScope())
        {
            parser.NumberOfSyntaxErrors.Should().Be(0, "while loop should be valid");
        }
    }

    [Test]
    public void ValidForLoop_ShouldHaveNoSyntaxErrors()
    {
        const string script = """
                              
                                          for (int i = 0; i < 10; i = i + 1;) {
                                              print(i);
                                          }
                                      
                              """;
        var parser = CreateParser(script);
        var tree = parser.script();

        using (new AssertionScope())
        {
            parser.NumberOfSyntaxErrors.Should().Be(0, "for loop should be valid");
        }
    }

    [Test]
    public void InvalidLogicalExpression_ShouldHaveSyntaxErrors()
    {
        const string script = "if (x &&) { print(x); }";
        var parser = CreateParser(script);
        var tree = parser.script();

        using (new AssertionScope())
        {
            parser.NumberOfSyntaxErrors.Should().BeGreaterThan(0, "logical expression is incomplete");
        }
    }

    [Test]
    public void ValidClassDefinition_ShouldHaveNoSyntaxErrors()
    {
        const string script = """

                              class Vector {
                                  int abas = 10;
                                  func int a(int val) {
                                      return val * abas;
                                  }
                              }

                              """;
        var parser = CreateParser(script);
        var tree = parser.script();

        using (new AssertionScope())
        {
            parser.NumberOfSyntaxErrors.Should().Be(0, "class definition should be valid");
        }
    }

    [Test]
    public void ValidClassInstantiationAndUsage_ShouldHaveNoSyntaxErrors()
    {
        const string script = """

                              class Vector {
                                  int abas = 10;
                                  func int a(int val) {
                                      return val + abas;
                                  }
                              }

                              Vector item = new Vector;
                              item.abas = 100;
                              print(item.a(10));

                              """;
        var parser = CreateParser(script);
        var tree = parser.script();

        using (new AssertionScope())
        {
            parser.NumberOfSyntaxErrors.Should().Be(0, "class instantiation and usage should be valid");
        }
    }

    [Test]
    public void MissingBraceInClassDefinition_ShouldHaveSyntaxErrors()
    {
        const string script = """

                              class MissingBrace {
                                  int x = 10;

                              // Missing closing brace

                              """;
        var parser = CreateParser(script);
        var tree = parser.script();

        using (new AssertionScope())
        {
            parser.NumberOfSyntaxErrors.Should()
                .BeGreaterThan(0, "missing brace in class definition should cause syntax errors");
        }
    }

    [Test]
    public void InvalidFieldAccess_ShouldHaveSyntaxErrors()
    {
        const string script = """

                              class Point {
                                  int x = 5;
                              }

                              Point p = new Point;
                              p.y = 10; // Field 'y' does not exist

                              """;
        var parser = CreateParser(script);
        var tree = parser.script();

        using (new AssertionScope())
        {
            parser.NumberOfSyntaxErrors.Should()
                .BeGreaterThan(0, "accessing an undefined field should cause syntax errors");
        }
    }

    [Test]
    public void MethodCallOnUndeclaredVariable_ShouldHaveSyntaxErrors()
    {
        const string script = """

                              class Dummy {
                                  func int getValue() {
                                      return 42;
                                  }
                              }

                              invalidVar.getValue(); // 'invalidVar' not declared

                              """;
        var parser = CreateParser(script);
        var tree = parser.script();

        using (new AssertionScope())
        {
            parser.NumberOfSyntaxErrors.Should()
                .BeGreaterThan(0, "calling method on undeclared variable should cause syntax errors");
        }
    }
}