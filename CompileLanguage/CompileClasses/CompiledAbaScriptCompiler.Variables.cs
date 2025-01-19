using AbaScript.AntlrClasses;
using CompileLanguage.Exceptions;

namespace CompileLanguage.CompileClasses;

public partial class CompiledAbaScriptCompiler
{
    public override object? VisitVariableDeclaration(AbaScriptParser.VariableDeclarationContext context)
    {
        if (context.expr() != null)
        {
            Visit(context.expr());
        }
        else
        {
            _stringBuilder.Append($"{Keywords.PUSH} 0");
        }

        _stringBuilder.Append($"{Keywords.STORE} {context.ID()}");
        return context;
    }

    public override object? VisitAssignment(AbaScriptParser.AssignmentContext context)
    {
        // работает только с интами
        if (context.expr().Length > 1)
        {
            throw new RuntimeException("can't work with arrays yet");
        }
        Visit(context.expr(0));
        _stringBuilder.Append($"{Keywords.STORE} {context.ID()}");

        return context;
    }
}