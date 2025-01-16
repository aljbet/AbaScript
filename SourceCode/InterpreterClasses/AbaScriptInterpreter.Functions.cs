using AbaScript.AntlrClasses;
using AbaScript.AntlrClasses.Models;

namespace AbaScript.InterpreterClasses;

public partial class AbaScriptInterpreter
{
    public override object VisitFunctionDef(AbaScriptParser.FunctionDefContext context)
    {
        var funcName = context.ID().GetText();
        var returnType = context.returnType().GetText();
        var parameters = context.typedParam().Select(p => (p.type().GetText(), p.ID().GetText())).ToList();

        functions[funcName] = (parameters, returnType, context.block());
        logger.Log(
            $"Функция {funcName} определена с параметрами: {string.Join(", ", parameters.Select(p => $"{p.Item1} {p.Item2}"))}");
        return null;
    }

    public override object VisitFuncCall(AbaScriptParser.FuncCallContext context)
    {
        var funcName = context.ID().GetText();

        if (!functions.TryGetValue(funcName, out var functionInfo))
            throw new InvalidOperationException($"Функция '{funcName}' не определена.");

        var arguments = context.expr().Select(expr => Visit(expr)).ToList();
        if (arguments.Count != functionInfo.Parameters.Count)
            throw new InvalidOperationException($"Количество аргументов не совпадает для функции '{funcName}'.");

        for (var i = 0; i < arguments.Count; i++)
        {
            var expectedType = functionInfo.Parameters[i].type;
            if (!CheckType(expectedType, arguments[i]))
                throw new InvalidOperationException(
                    $"Аргумент {i} функции {funcName} должен быть типа {expectedType}.");
        }

        // Сохраняем текущие переменные, чтобы не мешать глобальному состоянию
        var oldVariables = new Dictionary<string, Variable>(variables);

        variables.Clear();
        for (var i = 0; i < arguments.Count; i++)
        {
            var parameterType = Enum.Parse<VariableType>(functionInfo.Parameters[i].type, true);
            variables[functionInfo.Parameters[i].name] = new Variable(parameterType, arguments[i]);
        }

        try
        {
            Visit(functionInfo.Body);
        }
        catch (ReturnException ex)
        {
            if (!CheckType(functionInfo.ReturnType, ex.ReturnValue))
                throw new InvalidOperationException(
                    $"Возвращаемое значение функции {funcName} должно быть типа {functionInfo.ReturnType}.");

            return ex.ReturnValue;
        }
        finally
        {
            // Восстанавливаем переменные
            variables.Clear();
            foreach (var kvp in oldVariables)
                variables[kvp.Key] = kvp.Value;
        }

        return null;
    }

    public override object VisitReturnStatement(AbaScriptParser.ReturnStatementContext context)
    {
        var returnValue = Visit(context.expr());
        throw new ReturnException(returnValue);
    }
}