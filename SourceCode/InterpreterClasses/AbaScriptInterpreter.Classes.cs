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

        _classDefinitions[className] = classDef;
        _logger.Log($"Class '{className}' defined.");
        return null;
    }

    public override object VisitMethodCall(AbaScriptParser.MethodCallContext context)
    {
        var instanceName = context.ID(0).GetText();
        var methodName = context.ID(1).GetText();

        if (!_classInstances.TryGetValue(instanceName, out var instanceWrapper))
            throw new InvalidOperationException($"Instance '{instanceName}' does not exist.");

        var className = instanceWrapper.ClassName;

        if (!_classDefinitions.TryGetValue(className, out var classDefinition) ||
            !classDefinition.Methods.TryGetValue(methodName, out var methodInfo))
            throw new InvalidOperationException($"Method '{methodName}' not defined in class '{className}'.");


        var arguments = context.expr().Select(Visit).ToList();
        if (arguments.Count != methodInfo.Parameters.Count)
            throw new InvalidOperationException($"Incorrect number of arguments for method '{methodName}'.");

        for (var i = 0; i < arguments.Count; i++)
        {
            var expectedType = methodInfo.Parameters[i].type;
            if (!CheckType(expectedType, arguments[i]))
                throw new InvalidOperationException(
                    $"Argument {i + 1} of method '{methodName}' must be of type '{expectedType}'.");
        }


        var oldVariables = CaptureCurrentScope();

        for (var i = 0; i < arguments.Count; i++)
        {
            var parameterType = Enum.Parse<VariableType>(methodInfo.Parameters[i].type, true);
            _variables[methodInfo.Parameters[i].name] = new Variable(parameterType, arguments[i]);
        }
        
        try
        {
            Visit(methodInfo.Body);
        }
        catch (ReturnException ex)
        {
            if (!CheckType(methodInfo.ReturnType, ex.ReturnValue))
                throw new InvalidOperationException(
                    $"Return value of method '{methodName}' must be of type '{methodInfo.ReturnType}'.");

            RestoreScopeExcludingNewVariables(oldVariables);
            return ex.ReturnValue;
        }
        finally
        {
            RestoreScopeExcludingNewVariables(oldVariables);
        }

        return null;
    }
}