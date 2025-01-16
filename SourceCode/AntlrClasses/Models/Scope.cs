namespace AbaScript.AntlrClasses.Models;

public class Scope
{
    public Scope? parent;
    public Dictionary<string, AbaType> variables = new();

    public Scope(Scope? parent = null)
    {
        this.parent = parent;
    }

    public AbaType Resolve(string name)
    {
        if (variables.TryGetValue(name, out var resolve)) return resolve;

        return parent?.Resolve(name) ?? AbaType.Error;
    }
}