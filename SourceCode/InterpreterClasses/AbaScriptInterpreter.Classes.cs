using AbaScript.AntlrClasses;
using AbaScript.AntlrClasses.Models;

namespace AbaScript.InterpreterClasses;

public partial class AbaScriptInterpreter
{
    public override object VisitClassDef(AbaScriptParser.ClassDefContext context)
    {
        var className = context.ID().GetText();
        var classDef = new ClassDefinition();

        foreach (var member in context.classMember())
            if (member.variableDeclaration() != null)
            {
                var varType = member.variableDeclaration().type().GetText();
                var varName = member.variableDeclaration().ID().GetText();
                classDef.Fields[varName] = Enum.Parse<VariableType>(varType, true);
            }
            else if (member.functionDef() != null)
            {
                var funcName = member.functionDef().ID().GetText();
                var returnType = member.functionDef().returnType().GetText();
                var parameters = member.functionDef().typedParam()
                    .Select(p => (p.type().GetText(), p.ID().GetText()))
                    .ToList();
                classDef.Methods[funcName] = (parameters, returnType, member.functionDef().block());
            }

        classDefinitions[className] = classDef;
        logger.Log($"Класс {className} определен.");
        return null;
    }

    public override object VisitMethodCall(AbaScriptParser.MethodCallContext context)
    {
        var instanceName = context.ID(0).GetText();
        var methodName = context.ID(1).GetText();

        if (!classInstances.TryGetValue(instanceName, out var instance))
            throw new InvalidOperationException($"Экземпляр '{instanceName}' не существует.");

        var className = instance.GetType().Name;
        if (!classDefinitions[className].Methods.TryGetValue(methodName, out var methodInfo))
            throw new InvalidOperationException($"Метод '{methodName}' не определён в классе '{className}'.");

        var arguments = context.expr().Select<AbaScriptParser.ExprContext, object>(expr => Visit(expr)).ToList();
        if (arguments.Count != methodInfo.Parameters.Count)
            throw new InvalidOperationException($"Количество аргументов не совпадает для метода '{methodName}'.");

        for (var i = 0; i < arguments.Count; i++)
        {
            var expectedType = methodInfo.Parameters[i].type;
            if (!CheckType(expectedType, arguments[i]))
                throw new InvalidOperationException(
                    $"Аргумент {i} метода {methodName} должен быть типа {expectedType}.");
        }

        // Локальные переменные метода
        var oldVariables = new Dictionary<string, Variable>(variables);
        variables.Clear();

        // Инициализируем параметры
        for (var i = 0; i < arguments.Count; i++)
        {
            var parameterType = Enum.Parse<VariableType>(methodInfo.Parameters[i].type, true);
            variables[methodInfo.Parameters[i].name] = new Variable(parameterType, arguments[i]);
        }

        try
        {
            Visit(methodInfo.Body);
        }
        catch (ReturnException ex)
        {
            if (!CheckType(methodInfo.ReturnType, ex.ReturnValue))
                throw new InvalidOperationException(
                    $"Возвращаемое значение метода {methodName} должно быть типа {methodInfo.ReturnType}.");
            return ex.ReturnValue;
        }
        finally
        {
            variables.Clear();
            foreach (var kvp in oldVariables)
                variables[kvp.Key] = kvp.Value;
        }

        return null;
    }
}