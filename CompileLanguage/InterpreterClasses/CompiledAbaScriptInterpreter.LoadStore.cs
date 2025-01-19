using CompileLanguage.BaseAntlrClasses;
using CompileLanguage.Exceptions;

namespace CompileLanguage.InterpreterClasses;

public partial class CompiledAbaScriptInterpreter
{
    public override object? VisitLoadInstruction(CompiledAbaScriptParser.LoadInstructionContext context)
    {
        var varName = context.ID().GetText();
        if (!_variables.TryGetValue(varName, out var value))
        {
            throw new RuntimeException("Variable not found: " + varName);
        }

        _stack.Push(value);
        return null;
    }

    public override object? VisitStoreInstruction(CompiledAbaScriptParser.StoreInstructionContext context)
    {
        var varName = context.ID().GetText();
        if (_stack.Count == 0)
        {
            throw new RuntimeException("Stack underflow during STORE");
        }

        _variables[varName] = _stack.Pop();
        return null;
    }
}