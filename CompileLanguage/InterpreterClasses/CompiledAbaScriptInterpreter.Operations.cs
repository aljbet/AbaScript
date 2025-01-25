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
        PerformBinaryOperation((a, b) => b == 0 ? 0 : a % b);

    public override object? VisitAndInstruction(CompiledAbaScriptParser.AndInstructionContext context) =>
        PerformBinaryOperation((a, b) => a & b);

    public override object? VisitOrInstruction(CompiledAbaScriptParser.OrInstructionContext context) =>
        PerformBinaryOperation((a, b) => a | b);

    public override object? VisitNotInstruction(CompiledAbaScriptParser.NotInstructionContext context) =>
        PerformUnaryOperation((a) => (a != 0) ? 1 : 0);

    public override object? VisitEqInstruction(CompiledAbaScriptParser.EqInstructionContext context) =>
        PerformBinaryOperation((a, b) => (a == b) ? 1 : 0);

    public override object? VisitNeInstruction(CompiledAbaScriptParser.NeInstructionContext context) =>
        PerformBinaryOperation((a, b) => (a != b) ? 1 : 0);

    public override object? VisitLtInstruction(CompiledAbaScriptParser.LtInstructionContext context) =>
        PerformBinaryOperation((a, b) => (a < b) ? 1 : 0);

    public override object? VisitLeInstruction(CompiledAbaScriptParser.LeInstructionContext context) =>
        PerformBinaryOperation((a, b) => (a <= b) ? 1 : 0);

    public override object? VisitGtInstruction(CompiledAbaScriptParser.GtInstructionContext context) =>
        PerformBinaryOperation((a, b) => (a > b) ? 1 : 0);

    public override object? VisitGeInstruction(CompiledAbaScriptParser.GeInstructionContext context) =>
        PerformBinaryOperation((a, b) => (a >= b) ? 1 : 0);

    private object? PerformBinaryOperation(Func<long, long, long> operation)
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
    
    private object? PerformUnaryOperation(Func<long, long> operation)
    {
        if (_stack.Count < 1)
        {
            throw new RuntimeException("Stack underflow during unary operation");
        }
        
        var a = _stack.Pop();
        _stack.Push(operation(a));
        return null;
    }
}