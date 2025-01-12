namespace AbaScript.AntlrClasses.Models;

public class Scope
{
    public Dictionary<string, AbaType> variables = new Dictionary<string, AbaType>();
    public Scope? parent;

    public Scope(Scope? parent = null)
    {
        this.parent = parent;
    }

    public AbaType Resolve(string name)
    {
        if (variables.TryGetValue(name, out var resolve))
        {
            return resolve;
        }

        return parent?.Resolve(name) ?? AbaType.Error;
    }
}