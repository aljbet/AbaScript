using AbaScript.AntlrClasses;
using AbaScript.AntlrClasses.Exceptions;
using Antlr4.Runtime;
using FluentAssertions;

namespace Tests.AnalyzerTests;

[TestFixture]
public class TypeCheckerTests
{
    private void CheckType(string program)
    {
        var inputStream = new AntlrInputStream(program);
        var lexer = new AbaScriptLexer(inputStream);
        var tokenStream = new CommonTokenStream(lexer);
        var parser = new AbaScriptParser(tokenStream);
        var tree = parser.script(); // Ensure this matches your grammar's entry point
        var visitor = new AbaScriptTypeChecker();
        visitor.Visit(tree);
    }

    private void AssertTypeException(string program, string expectedMessage)
    {
        var exception = Assert.Throws<TypeCheckerException>(() => CheckType(program));
        exception.Message.Should().Be(expectedMessage);
    }

    [Test]
    public void VisitFunctionDef_ValidFunction_NoException()
    {
        var program = @"int add(int a, int b) { return a + b; }";
        CheckType(program); // Expecting no exception
    }

    [Test]
    public void VisitFuncCall_ValidCall_NoException()
    {
        var program = @"int add(int a, int b) { return a + b; } int result = add(5, 3);";
        CheckType(program);
    }

    [Test]
    public void VisitFuncCall_IncorrectNumberOfArguments_ThrowsException()
    {
        var program = @"int add(int a, int b) { return a + b; } int result = add(5);";
        AssertTypeException(program, "Incorrect number of arguments for function 'add'. Expected 2, got 1.");
    }

    [Test]
    public void VisitClassDef_ValidClass_NoException()
    {
        var program = @"
                class MyClass {
                    int x;
                    string y;

                    int getX() {
                        return x;
                    }
                }

                MyClass instance = new MyClass();
                int val = instance.x;";
        CheckType(program);
    }

    [Test]
    public void VisitFieldAccess_InvalidField_ThrowsException()
    {
        var program = @"
                class MyClass {
                    int x;
                }

                MyClass instance = new MyClass();
                string val = instance.y;"; // Accessing non-existent field 'y'
        AssertTypeException(program, "Class 'MyClass' does not contain field 'y'.");
    }

    [Test]
    public void VisitVariableDeclaration_ValidDeclaration_NoException()
    {
        var program = @"int x = 5;";
        CheckType(program);
    }

    [Test]
    public void VisitVariableDeclaration_InvalidDeclaration_ThrowsException()
    {
        var program = @"int x = ""hello"";";
        AssertTypeException(program, "Type mismatch in variable declaration. Expected 'int', but got 'string'.");
    }

    [Test]
    public void VisitAssignment_ValidAssignment_NoException()
    {
        var program = @"int x = 5; x = 10;";
        CheckType(program);
    }

    [Test]
    public void VisitAssignment_InvalidAssignment_ThrowsException()
    {
        var program = @"int x = 5; x = ""hello"";";
        AssertTypeException(program, "Type mismatch in assignment. Cannot assign 'string' to 'int'.");
    }

    [Test]
    public void VisitAddSub_ValidAddSub_NoException()
    {
        var program = "int x = 5 + 3;";
        CheckType(program);
    }

    [Test]
    public void VisitAddSub_InvalidAddSub_ThrowsException()
    {
        var program = "int x = 5 + \"hello\";";
        AssertTypeException(program, "Type mismatch: both operands of '+' or '-' must be integers. Got int and string");
    }

    [Test]
    public void VisitMulDivMod_ValidMulDivMod_NoException()
    {
        var program = "int x = 5 * 3;";
        CheckType(program);
    }

    [Test]
    public void VisitMulDivMod_InvalidMulDivMod_ThrowsException()
    {
        var program = "int x = 5 * \"hello\";";
        AssertTypeException(program,
            "Type mismatch: both operands of '*', '/', or '%' must be integers. Got int and string");
    }

    [Test]
    public void VisitReturnType_ValidReturnType_NoException()
    {
        var program = @"int myFunc() { return 5; }";
        CheckType(program);
    }
}