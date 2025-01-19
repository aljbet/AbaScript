using CompileLanguage.BaseAntlrClasses;
using CompileLanguage.Exceptions;

namespace CompileLanguage.InterpreterClasses;

public partial class CompiledAbaScriptInterpreter
{
    public override object? VisitAddInstruction(CompiledAbaScriptParser.AddInstructionContext context) =>
        PerformBinaryOperation((a, b) => a + b);

    public override object? VisitSubInstruction(CompiledAbaScriptParser.SubInstructionContext context) =>
        PerformBinaryOperation((a, b) => a - b);

    public override object? VisitMulInstruction(CompiledAbaScriptParser.MulInstructionContext context) =>
        PerformBinaryOperation((a, b) => a * b);

    public override object? VisitDivInstruction(CompiledAbaScriptParser.DivInstructionContext context) =>
        PerformBinaryOperation((a, b) => a / b);

    public override object? VisitModInstruction(CompiledAbaScriptParser.ModInstructionContext context) =>
        PerformBinaryOperation((a, b) => a % b);


    private object? PerformBinaryOperation(Func<int, int, int> operation)
    {
        if (_stack.Count < 2)
        {
            throw new RuntimeException("Stack underflow during binary operation");
        }

        var b = _stack.Pop();
        var a = _stack.Pop();
        _stack.Push(operation(a, b));
        return null;
    }
}