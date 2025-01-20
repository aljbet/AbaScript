using AbaScript.AntlrClasses;
using CompileLanguage.InterpreterClasses;

namespace CompileLanguage.CompileClasses;

public partial class CompiledAbaScriptCompiler
{
    public override object VisitClassDef(AbaScriptParser.ClassDefContext context)
    {
        var className = context.ID().GetText();
        var classDef = new ClassInfo();

        foreach (var member in context.classMember())
        {
            if (member.variableDeclaration() != null)
            {
                var varType = member.variableDeclaration().type().GetText();
                var varName = member.variableDeclaration().ID().GetText();
                classDef.Fields.Add(new FieldInfo(varType, varName));
            }
            else if (member.functionDef() != null)
            {
                VisitMethodDef(member.functionDef(), className);
            }
        }

        _classContexts.Add(className, classDef);
        return context;
    }

    public override object VisitMethodCall(AbaScriptParser.MethodCallContext context)
    {
        var instanceName = context.ID(0).GetText();
        var methodName = context.ID(1).GetText();

        if (!_classInstances.TryGetValue(instanceName, out var instance))
            throw new InvalidOperationException($"Экземпляр '{instanceName}' не существует.");

        var className = instance.GetType().Name;
        methodName = className + "." + methodName;
        if (!_functions.ContainsKey(methodName))
            throw new InvalidOperationException($"Метод '{methodName}' не определён в классе '{className}'.");

        foreach (var expr in context.expr())
        {
            Visit(expr);
        }

        _stringBuilder.AppendLine($"{Keywords.LOAD} {instanceName} {className}");
        _stringBuilder.AppendLine($"{Keywords.CALL} {methodName}");

        return context;
    }

    private object VisitMethodDef(AbaScriptParser.FunctionDefContext context, string className)
    {
        var funcName = className + "." + context.ID().GetText();
        var returnType = context.returnType().GetText();
        var parameters = context.typedParam().Select(p => (p.type().GetText(), p.ID().GetText())).ToList();

        _stringBuilder.AppendLine(funcName + ":");

        _stringBuilder.AppendLine($"{Keywords.INIT} {className}_object {className}");
        _stringBuilder.AppendLine($"{Keywords.STORE} {className}_object");

        for (var i = context.typedParam().Length - 1; i >= 0; i--)
        {
            _stringBuilder.AppendLine($"{Keywords.STORE} {funcName}");
        }

        VisitBlock(context.block());

        _stringBuilder.AppendLine(Keywords.RET); // Лишний RET не помешает

        _functions[funcName] = (parameters, returnType, context.block());
        return context;
    }
}