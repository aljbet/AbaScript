namespace AbaScript.AntlrClasses.Models;

public class ClassDefinition
{
    public Dictionary<string, VariableType> Fields { get; } = new();
    public Dictionary<string, (List<(string type, string name)> Parameters, string ReturnType, AbaScriptParser.BlockContext Body)> Methods { get; } = new();
}

public class ClassInstance
{
    public Dictionary<string, object> Fields { get; } = new();
}