using CompileLanguage.BaseAntlrClasses;
using CompileLanguage.Exceptions;

namespace CompileLanguage.InterpreterClasses;

public partial class CompiledAbaScriptInterpreter
{
    public override object? VisitPushInstruction(CompiledAbaScriptParser.PushInstructionContext context)
    {
        _stack.Push(int.Parse(context.value().NUMBER().GetText()));
        return null;
    }

    public override object? VisitPopInstruction(CompiledAbaScriptParser.PopInstructionContext context)
    {
        if (_stack.Count == 0)
        {
            throw new RuntimeException("Stack underflow during POP");
        }
        _stack.Pop();
        return null;
    }
}