using System.Text;
using AbaScript.AntlrClasses;
using AbaScript.AntlrClasses.Models;
using CompileLanguage.Exceptions;
using CompileLanguage.InterpreterClasses;
using CompileLanguage.Services;

namespace CompileLanguage.CompileClasses;

public partial class CompiledAbaScriptCompiler : AbaScriptBaseVisitor<object?>
{
    private readonly StringBuilder _stringBuilder = new();
    private readonly IVariableStorage _variableStorage = new VariableStorage();
    private Dictionary<string, ClassInfo> _classContexts = new();
    private readonly Dictionary<string, ClassInstance> _classInstances = new();
    private int _ifBlocksCount = 0;
    private int _forBlocksCount = 0;

    private readonly Dictionary<string, (List<(string type, string name)> Parameters, string ReturnType,
        AbaScriptParser.BlockContext Body)> _functions
        = new();

    public override object VisitScript(AbaScriptParser.ScriptContext context)
    {
        SetUpService();

        foreach (var classDef in context.classDef())
        {
            Visit(classDef);
        }
        
        var mainFunction = context.functionDef().FirstOrDefault(func => func.ID().GetText() == "main");
        if (mainFunction == null)
        {
            throw new RuntimeException("No main function.");
        }

        foreach (var funcDef in context.functionDef())
        {
            Visit(funcDef);
        }

        return _stringBuilder.ToString();
    }

    public string GetActualCompiledCode()
    {
        return _stringBuilder.ToString();
    }

    public Dictionary<string, ClassInfo> GetClassesContext()
    {
        return _classContexts;
    }

    private void SetUpService()
    {
        _stringBuilder.Clear();
        _variableStorage.Clear();
    }
}