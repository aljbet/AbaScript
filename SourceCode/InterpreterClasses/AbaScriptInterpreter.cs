using AbaScript.AntlrClasses;
using AbaScript.AntlrClasses.Models;

namespace AbaScript.InterpreterClasses;

public partial class AbaScriptInterpreter : AbaScriptBaseVisitor<object>
{
    private readonly Dictionary<string, ClassDefinition> classDefinitions = new();
    private readonly Dictionary<string, ClassInstance> classInstances = new();

    private readonly Dictionary<string, (List<(string type, string name)> Parameters, string ReturnType,
        AbaScriptParser.BlockContext Body)> functions
        = new();

    private readonly Logger logger = new();
    private readonly Dictionary<string, Variable> variables = new();

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
}