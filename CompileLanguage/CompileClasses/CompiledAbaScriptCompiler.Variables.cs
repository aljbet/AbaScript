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
            // TODO: тут нет INIT
            var count = int.Parse(context.NUMBER().GetText());
            CreateEmptyArray(id, count);
            return null;
        }

        _stringBuilder.AppendLine($"{Keywords.INIT} {context.ID()} {context.type().GetText()}");

        if (context.expr() != null)
        {
            Visit(context.expr());
            _stringBuilder.AppendLine($"{Keywords.STORE} {context.ID()}");
        }
        
        // TODO: понять, нужно ли это
        // _variableStorage.SaveVariable(id, new IntVariable()
        // {
        //     Value = expressionResult;
        // });

        return context;
    }

    private void CreateEmptyArray(string id, int count)
    {
        for (var i = 0; i < count; i++)
        {
            _stringBuilder.AppendLine($"{Keywords.PUSH} 0");
            _stringBuilder.AppendLine($"{Keywords.STORE} {id}[{i}]");
        }
        
        // TODO: понять, нужно ли это
        // _variableStorage.SaveVariable(id, new ArrayVariable
        // {
        //     Value = new int[count]
        // });
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

    public override object? VisitVariableOrArrayAccess(AbaScriptParser.VariableOrArrayAccessContext context)
    {
        string variableName = context.ID().GetText();
        if (context.expr() != null) // array element
        {
            throw new RuntimeException("can't work with arrays yet");
        }
        else // int or array
        {
            // здесь же окажется запрос доступа к x не только как к int, но и как к "указателю" на массив (x[i] обработано в array element)
            _stringBuilder.AppendLine($"{Keywords.LOAD} {variableName}");
        }

        return context;
    }

    public override object? VisitNumber(AbaScriptParser.NumberContext context)
    {
        if (long.TryParse(context.GetText(), out var number))
        {
            _stringBuilder.AppendLine($"{Keywords.PUSH} {number}");
            
            return context;
        }

        throw new InvalidOperationException($"Невозможно преобразовать в число: {context.GetText()}");
    }
}