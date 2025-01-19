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

        if (_functions.ContainsKey(funcName))
        {
            throw new InvalidOperationException($"Function '{funcName}' already defined.");
        }
        _functions[funcName] = (parameters, returnType, context.block());
        _logger.Log(
            $"Функция {funcName} определена с параметрами: {string.Join(", ", parameters.Select(p => $"{p.Item1} {p.Item2}"))}");
        return null;
    }

    public override object VisitFuncCall(AbaScriptParser.FuncCallContext context)
    {
        var funcName = context.ID().GetText();

        if (!_functions.TryGetValue(funcName, out var functionInfo))
            throw new InvalidOperationException($"Function '{funcName}' not defined.");

        var arguments = context.expr().Select(expr => Visit(expr)).ToList();
        if (arguments.Count != functionInfo.Parameters.Count)
            throw new InvalidOperationException($"Incorrect number of arguments for function '{funcName}'.");

        for (var i = 0; i < arguments.Count; i++)
        {
            var expectedType = functionInfo.Parameters[i].type;
            if (!CheckType(expectedType, arguments[i]))
                throw new InvalidOperationException(
                    $"Argument {i + 1} of function '{funcName}' must be of type '{expectedType}'.");
        }

        var oldVariables = CaptureCurrentScope();

        for (var i = 0; i < arguments.Count; i++)
        {
            var parameterType = Enum.Parse<VariableType>(functionInfo.Parameters[i].type, true);
            _variables[functionInfo.Parameters[i].name] = new Variable(parameterType, arguments[i]);
        }

        try
        {
            return Visit(functionInfo.Body);
        }
        catch (ReturnException ex)
        {
            if (!CheckType(functionInfo.ReturnType, ex.ReturnValue))
                throw new InvalidOperationException(
                    $"Return value of function '{funcName}' must be of type '{functionInfo.ReturnType}'.");

            RestoreScopeExcludingNewVariables(oldVariables);
            return ex.ReturnValue;
        }
        finally
        {
            RestoreScopeExcludingNewVariables(oldVariables);
        }
    }

    public override object VisitReturnStatement(AbaScriptParser.ReturnStatementContext context)
    {
        var returnValue = Visit(context.expr());
        throw new ReturnException(returnValue);
    }
}