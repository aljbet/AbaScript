namespace AbaScript.AntlrClasses;

public class Variable
{
    public VariableType Type { get; }
    public object Value { get; set; }
    
    public Variable(VariableType type, object value)
    {
        Type = type;
        Value = value;
    }
}