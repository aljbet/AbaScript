namespace AbaScript.AntlrClasses.Models;

public class AbaType
{
    public AbaType(string name, bool isArray = false)
    {
        Name = name;
        IsArray = isArray;
    }

    public string Name { get; }
    public bool IsArray { get; set; }

    public static AbaType Int => new("int");
    public static AbaType String => new("string");
    public static AbaType Bool => new("bool");
    public static AbaType Void => new("void");
    public static AbaType Error => new("error");

    public static AbaType Class(string className)
    {
        return new AbaType(className);
    }

    public static AbaType Array(AbaType elementType)
    {
        return new AbaType(elementType.Name, true);
    }

    public override bool Equals(object obj)
    {
        if (obj is not AbaType other) return false;
        return Name == other.Name && IsArray == other.IsArray;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Name, IsArray);
    }

    public override string ToString()
    {
        return IsArray ? $"{Name}[]" : Name;
    }
}