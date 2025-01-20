namespace CompileLanguage.InterpreterClasses;

public enum StorageType
{
    Heap,
    Stack
}

public class Variable
{
    public Variable()
    {
        
    }
    public Variable(string type, int address, StorageType storageType, string name)
    {
        Type = type;
        Address = address;
        StorageType = storageType;
        Name = name;
    }

    public string Type { get; set; }
    public int Address { get; set; }
    public StorageType StorageType { get; set; } 
    public string Name { get; set; }
}

public class FieldInfo
{
    public FieldInfo(){}
    public FieldInfo(string type, string name)
    {
        Type = type;
        Name = name;
    }

    public string Type { get; set; }
    public string Name { get; set; }
}

// этот контекст можно будет заранее обработать
public class ClassInfo
{
    public ClassInfo() {}
    public ClassInfo(List<FieldInfo> fieldInfos)
    {
        Fields = fieldInfos;
    }
    public List<FieldInfo> Fields { get; set; } = new List<FieldInfo>();
}

public struct SimpleTypes
{
    public static string boolType = "bool";
    public static string intType = "int";
    public static string stringType = "string";

    public static bool IsSimple(String t)
    {
        return t == boolType || t == intType || t == stringType;
    }
}