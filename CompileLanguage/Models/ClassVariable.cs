using AbaScript.AntlrClasses;

namespace CompileLanguage.Models;

public class ClassVariable
{
    public string Name { get; set; }
    public Dictionary<string, IntVariable> IntFields { get; set; } = new();
    public Dictionary<string, ArrayVariable> ArrayFields { get; set; } = new();
    public Dictionary<string, ClassVariable> ClassFields { get; set; } = new();
    public Dictionary<string, AbaScriptParser.FunctionDefContext> Methods { get; set; } = new();
}