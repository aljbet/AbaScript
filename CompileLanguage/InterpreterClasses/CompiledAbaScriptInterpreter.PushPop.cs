using CompileLanguage.Exceptions;

namespace CompileLanguage.InterpreterClasses;

public partial class CompiledAbaScriptInterpreter
{
    public override object? VisitPushInstruction(CompiledAbaScriptParser.PushInstructionContext ctx)
    {
        _stack.Push(int.Parse(ctx.value().NUMBER().GetText()));
        return null;
    }

    public override object? VisitPopInstruction(CompiledAbaScriptParser.PopInstructionContext ctx)
    {
        if (_stack.Count == 0)
        {
            throw new RuntimeException("Stack underflow during POP");
        }
        _stack.Pop();
        return null;
    }
}