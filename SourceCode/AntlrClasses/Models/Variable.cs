namespace AbaScript.AntlrClasses;

public class Variable(VariableType type, object value, string className = null)
{
    public VariableType Type { get; } = type;
    public object Value { get; set; } = value;
    public string ClassName { get; } = className;
}