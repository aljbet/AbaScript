using CompileLanguage.Exceptions;

namespace CompileLanguage.InterpreterClasses;

public partial class CompiledAbaScriptInterpreter
{
    public override object? VisitLoadInstruction(CompiledAbaScriptParser.LoadInstructionContext ctx)
    {
        var varName = ctx.ID().GetText();
        if (!_variables.ContainsKey(varName))
        {
            throw new RuntimeException("Variable not found: " + varName);
        }
        _stack.Push(_variables[varName]);
        return null;
    }

    public override object? VisitStoreInstruction(CompiledAbaScriptParser.StoreInstructionContext ctx)
    {
        var varName = ctx.ID().GetText();
        if (_stack.Count == 0)
        {
            throw new RuntimeException("Stack underflow during STORE");
        }
        _variables[varName] = _stack.Pop();
        return null;
    }
}