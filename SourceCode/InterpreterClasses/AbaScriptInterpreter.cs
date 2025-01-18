using AbaScript.AntlrClasses;
using AbaScript.AntlrClasses.Models;

namespace AbaScript.InterpreterClasses;

public partial class AbaScriptInterpreter : AbaScriptBaseVisitor<object>
{
    private readonly Dictionary<string, ClassDefinition> _classDefinitions = new();
    private readonly Dictionary<string, ClassInstance> _classInstances = new();

    private readonly Dictionary<string, (List<(string type, string name)> Parameters, string ReturnType,
        AbaScriptParser.BlockContext Body)> _functions = new();

    private readonly Logger _logger = new();
    private readonly Dictionary<string, Variable> _variables = new();

    public override object VisitScript(AbaScriptParser.ScriptContext context)
    {
        foreach (var classDefContext in context.classDef())
        {
            Visit(classDefContext);
        }

        foreach (var functionDefContext in context.functionDef())
        {
            Visit(functionDefContext);
        }

        if (!_functions.ContainsKey("main"))
        {
            throw new InvalidOperationException("No main function found.");
        }

        var mainFunctionInfo = _functions["main"];
        if (mainFunctionInfo.Parameters.Count > 0)
        {
            throw new InvalidOperationException("Main function should not have any parameters.");
        }

        _variables.Clear();
        Visit(mainFunctionInfo.Body);

        return null;
    }

    private static bool CheckType(string type, object? value)
    {
        if (Enum.TryParse<VariableType>(type, true, out var enumType))
        {
            switch (enumType)
            {
                case VariableType.Int:
                    return value is int;
                case VariableType.String:
                    return value is string;
                case VariableType.Array:
                    return value is object[];
                case VariableType.Class:
                    return value != null;
            }
        }

        if (value is ClassInstance instance)
            return instance.ClassName == type;
        return false;
    }

    private object TryParseNumber(string input)
    {
        if (int.TryParse(input, out var number)) return number;

        return input; // возврат как строку, если не удалось привести к int
    }

    private Dictionary<string, Variable> CaptureCurrentScope()
    {
        return _variables.ToDictionary(entry => entry.Key, entry => entry.Value);
    }

    private void RestoreScopeExcludingNewVariables(Dictionary<string, Variable> oldVariables)
    {
        var keysToRemove = _variables.Keys.Except(oldVariables.Keys).ToList();
        foreach (var key in keysToRemove)
        {
            _variables.Remove(key);
        }

        foreach (var kvp in oldVariables.Where(kvp => _variables.ContainsKey(kvp.Key)))
        {
            _variables[kvp.Key] = kvp.Value;
        }
    }
}