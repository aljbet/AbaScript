using AbaScript.AntlrClasses;
using Antlr4.Runtime;

namespace Tests.AnalyzerTests;

public class MainCheckerTests
{
    private static AbaScriptMainChecker CreateAnalyzer(string script)
    {
        var inputStream = new AntlrInputStream(script);
        var lexer = new AbaScriptLexer(inputStream);
        var tokenStream = new CommonTokenStream(lexer);
        var parser = new AbaScriptParser(tokenStream);
        var tree = parser.script();

        var analyzer = new AbaScriptMainChecker();
        analyzer.Visit(tree);
        return analyzer;
    }

    [Test]
    public void TestMainFunctionExists()
    {
        var script = "func int main() { int x = 0; }";
        var analyzer = CreateAnalyzer(script);
        Assert.IsTrue(analyzer.HasMainFunction(), "Main function should be detected.");
    }

    [Test]
    public void TestMainFunctionDoesNotExist()
    {
        var script = "void someFunction() { int x = 0; }";
        var analyzer = CreateAnalyzer(script);
        Assert.IsFalse(analyzer.HasMainFunction(), "Main function should not be detected.");
    }

    [Test]
    public void TestNoGlobalVariables()
    {
        var script = "func int main() { int x = 0; }";
        var analyzer = CreateAnalyzer(script);
        Assert.IsEmpty(analyzer.GetGlobalVariables(), "There should be no global variables.");
    }

    [Test]
    public void TestGlobalVariablesExist()
    {
        var script = "int x; int main() { int y = 0; }";
        var analyzer = CreateAnalyzer(script);
        Assert.IsNotEmpty(analyzer.GetGlobalVariables(), "Global variables should be detected.");
        Assert.IsTrue(analyzer.GetGlobalVariables().Contains("x"), "Global variable 'x' should be detected.");
    }
}