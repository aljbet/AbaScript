using CompileLanguage.Exceptions;

namespace CompileLanguage.InterpreterClasses;

public partial class CompiledAbaScriptInterpreter
{
    private int _jumpDestination = -1;
    public override object? VisitJmpInstruction(CompiledAbaScriptParser.JmpInstructionContext ctx) => JumpToLabel(ctx.labelRef().ID().GetText());

    public override object? VisitJmpIfInstruction(CompiledAbaScriptParser.JmpIfInstructionContext ctx)
    {
        if (_stack.Count > 0 && _stack.Pop() != 0)
        {
            JumpToLabel(ctx.labelRef().ID().GetText());
        }
        return null;
    }

    private object? JumpToLabel(string labelName)
    {
        if (!_labels.ContainsKey(labelName))
        {
            throw new RuntimeException("Label not found: " + labelName);
        }
        _jumpDestination = _labels[labelName];
        return null;
    }


    public override object? VisitPrintInstruction(CompiledAbaScriptParser.PrintInstructionContext ctx)
    {
        if (_stack.Count == 0)
        {
            throw new RuntimeException("Stack underflow during PRINT");
        }
        Console.WriteLine(_stack.Peek());
        return null;
    }

    public override object? VisitHaltInstruction(CompiledAbaScriptParser.HaltInstructionContext ctx)
    {
        _statements = new List<CompiledAbaScriptParser.StatementContext>();
        return null;
    }
}