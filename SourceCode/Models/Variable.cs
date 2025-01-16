namespace AbaScript.Models;

public class Variable : IHaveName
{
    public Variable(string name)
    {
        Name = name;
    }
    public string Name { get; set; }
    public string GetName()
    {
        return Name;
    }
}