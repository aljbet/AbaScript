using AbaScript.AntlrClasses;
using CompileLanguage.Exceptions;
using CompileLanguage.Models;

namespace CompileLanguage.CompileClasses;

public partial class CompiledAbaScriptCompiler
{
    public override object? VisitVariableDeclaration(AbaScriptParser.VariableDeclarationContext context)
    {
        var id = context.ID().GetText();
        if (context.NUMBER() != null)
        {
            var count = int.Parse(context.NUMBER().GetText());
            CreateEmptyArray(id, count);
            return null;
        }

        var expressionResult = 0;
        if (context.expr() != null)
        {
            expressionResult = (int)(Visit(context.expr()) ?? 0);
        }
        
        
        _stringBuilder.AppendLine($"{Keywords.PUSH} {expressionResult}");
        _stringBuilder.AppendLine($"{Keywords.STORE} {context.ID()}");
        
        _variableStorage.SaveVariable(id, new IntVariable()
        {
            Value = expressionResult
        });
        return context;
    }

    private void CreateEmptyArray(string id, int count)
    {
        for (var i = 0; i < count; i++)
        {
            _stringBuilder.AppendLine($"{Keywords.PUSH} 0");
            _stringBuilder.AppendLine($"{Keywords.STORE} {id}[{i}]");
        }
        _variableStorage.SaveVariable(id, new ArrayVariable
        {
            Value = new int[count]
        });
    }

    public override object? VisitAssignment(AbaScriptParser.AssignmentContext context)
    {
        // работает только с интами
        if (context.expr().Length > 1)
        {
            throw new RuntimeException("can't work with arrays yet");
        }
        Visit(context.expr(0));
        _stringBuilder.AppendLine($"{Keywords.STORE} {context.ID()}");

        return context;
    }
}