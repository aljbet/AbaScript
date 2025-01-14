using AbaScript.AntlrClasses;
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
    public void ValidLogicalNotUsage_ShouldHaveNoSyntaxErrors()
    {
        const string script = """
                              int x = 5;
                              if (!(x == 5)) {
                                  print(0);
                              } else {
                                  print(1);
                              }
                              """;

        var parser = CreateParser(script);
        var tree = parser.script();

        using (new AssertionScope())
        {
            parser.NumberOfSyntaxErrors.Should().Be(0, "logical NOT with parentheses should be valid");
        }
    }

    [Test]
    public void InvalidLogicalNotUsage_ShouldHaveSyntaxErrors()
    {
        const string script = """
                              int x = 5;
                              if (!x == 5) {
                                  print(x);
                              }
                              """;

        var parser = CreateParser(script);
        var tree = parser.script();

        using (new AssertionScope())
        {
            parser.NumberOfSyntaxErrors.Should()
                .BeGreaterThan(0, "misuse of logical NOT operator should cause syntax errors");
        }
    }

    [Test]
    public void ValidLogicalExpressionWithParentheses_ShouldHaveNoSyntaxErrors()
    {
        const string script = """
                              int x = 3;
                              int y = 7;
                              if ((x > 0 && y < 10) || !(y == 7)) {
                                  print("Expression true");
                              }
                              """;

        var parser = CreateParser(script);
        var tree = parser.script();

        using (new AssertionScope())
        {
            parser.NumberOfSyntaxErrors.Should().Be(0, "complex parentheses in logical expression should be valid");
        }
    }

    [Test]
    public void NestedIfElse_ShouldHaveNoSyntaxErrors()
    {
        const string script = """
                              
                                          if (x > 0) {
                                              if (y < 0) {
                                                  print("x is positive and y is negative");
                                              } else {
                                                  print("x is positive and y is non-negative");
                                              }
                                          } else {
                                              print("x is non-positive");
                                          }
                                      
                              """;

        var parser = CreateParser(script);
        var tree = parser.script();

        using (new AssertionScope())
        {
            parser.NumberOfSyntaxErrors.Should().Be(0, "nested if-else statements should be valid");
        }
    }

    [Test]
    public void ComplexArithmeticExpression_ShouldHaveNoSyntaxErrors()
    {
        const string script = "int result = (a + b) * (c - d) / e;";
        var parser = CreateParser(script);
        var tree = parser.script();

        using (new AssertionScope())
        {
            parser.NumberOfSyntaxErrors.Should().Be(0, "complex arithmetic expression should be valid");
        }
    }

    [Test]
    public void InvalidNestedFunctionCall_ShouldHaveSyntaxErrors()
    {
        const string script = "print(add(multiply(x, y), z);";
        var parser = CreateParser(script);
        var tree = parser.script();

        using (new AssertionScope())
        {
            parser.NumberOfSyntaxErrors.Should()
                .BeGreaterThan(0, "missing closing parenthesis in nested function call");
        }
    }

    [Test]
    public void ValidArrayDeclarationAndAccess_ShouldHaveNoSyntaxErrors()
    {
        const string script = """
                              
                                          int numbers[5];
                                          print(numbers[2]);
                                      
                              """;
        var parser = CreateParser(script);
        var tree = parser.script();

        using (new AssertionScope())
        {
            parser.NumberOfSyntaxErrors.Should().Be(0, "array declaration and access should be valid");
        }
    }

    [Test]
    public void InvalidArrayAccess_ShouldHaveSyntaxErrors()
    {
        const string script = "print(numbers[5);";
        var parser = CreateParser(script);
        var tree = parser.script();

        using (new AssertionScope())
        {
            parser.NumberOfSyntaxErrors.Should().BeGreaterThan(0, "missing closing bracket in array access");
        }
    }
}