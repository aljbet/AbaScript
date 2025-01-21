using CompileLanguage.BaseAntlrClasses;
using CompileLanguage.Exceptions;

namespace CompileLanguage.InterpreterClasses;

public partial class CompiledAbaScriptInterpreter
{
    private int _jumpDestination = -1;

    public override object? VisitJmpInstruction(CompiledAbaScriptParser.JmpInstructionContext context) =>
        JumpToLabel(context.labelRef().ID().GetText());

    public override object? VisitIfThenElseInstruction(CompiledAbaScriptParser.IfThenElseInstructionContext context)
    {
        if (_stack.Count == 0)
        {
            throw new RuntimeException("Stack empty.");
        }

        var value = _stack.Pop();
        if (value != 0)
        {
            JumpToLabel(context.labelRef(0).ID().GetText());
        }
        else
        {
            JumpToLabel(context.labelRef(1).ID().GetText());
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


    public override object? VisitPrintInstruction(CompiledAbaScriptParser.PrintInstructionContext context)
    {
        if (_stack.Count == 0)
        {
            throw new RuntimeException("Stack underflow during PRINT");
        }

        Console.WriteLine(_stack.Peek());
        return null;
    }

    public override object? VisitHaltInstruction(CompiledAbaScriptParser.HaltInstructionContext context)
    {
        _statements = new List<CompiledAbaScriptParser.StatementContext>();
        return null;
    }
}