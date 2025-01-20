using AbaScript.AntlrClasses;

namespace CompileLanguage.CompileClasses;

public partial class CompiledAbaScriptCompiler
{
    public override object? VisitFunctionDef(AbaScriptParser.FunctionDefContext context)
    {
        var funcName = context.ID().GetText();
        var returnType = context.returnType().GetText();
        var parameters = context.typedParam().Select(p => (p.type().GetText(), p.ID().GetText())).ToList();
        
        _stringBuilder.AppendLine(funcName + ":");
        _stringBuilder.AppendLine(Keywords.ENTER_SCOPE);
        for (var i = context.typedParam().Length - 1; i >= 0; i--)
        {
            _stringBuilder.AppendLine($"{Keywords.INIT} {context.typedParam()[i].ID().GetText()} {context.typedParam()[i].type().GetText()}");
            _stringBuilder.AppendLine($"{Keywords.STORE} {context.typedParam()[i].ID().GetText()}"); // Развернутое вытаскивание элементов со стека
        }

        VisitBlock(context.block());
        
        _stringBuilder.AppendLine(Keywords.RET); // Лишний RET не помешает
        _stringBuilder.AppendLine(Keywords.EXIT_SCOPE);
        
        _functions[funcName] = (parameters, returnType, context.block());
        return context;
    }

    public override object? VisitFuncCall(AbaScriptParser.FuncCallContext context)
    {
        var funcName = context.ID().GetText();

        if (!_functions.TryGetValue(funcName, out var functionInfo))
            throw new InvalidOperationException($"Функция '{funcName}' не определена.");

        foreach (var expr in context.expr())
        {
            Visit(expr); // Забил на обработку ошибок, засовывание в стек элементов
        }
        _stringBuilder.AppendLine($"{Keywords.CALL} {funcName}");
        
        return null;
    }

    public override object? VisitBlock(AbaScriptParser.BlockContext context)
    {
        foreach (var statement in context.statement())
        {
            Visit(statement);
        }
        
        return context;
    }

    public override object? VisitReturnStatement(AbaScriptParser.ReturnStatementContext context)
    {
        Visit(context.expr());
        _stringBuilder.AppendLine(Keywords.RET);
        return null;
    }
}