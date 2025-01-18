using AbaScript.AntlrClasses;
using AbaScript.InterpreterClasses;
using Antlr4.Runtime;
using FluentAssertions;

namespace Tests.InterpreterTests;

[TestFixture]
public class ExpressionsVisitorOutputTests : InterpreterTestBase
{
    [Test]
    public void ShouldPrintAdditionResult()
    {
        const string code = @"
            func int main() {
                int result = 2 + 3;
                print(result);
            }
        ";

        ExecuteCode(code, output => output.Should().Be("5"));
    }

    [Test]
    public void ShouldThrowOnInvalidExpression()
    {
        const string code = @"
            func int main() {
                int x = 2 + ""string"";
            }
        ";
        
        ExecuteCode(code, output =>
            FluentActions.Invoking(() => output)
                .Should().Throw<InvalidOperationException>());
    }
}