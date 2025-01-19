using System.Text;
using AbaScript.AntlrClasses;
using CompileLanguage.Services;

namespace CompileLanguage.CompileClasses;

public partial class CompiledAbaScriptCompiler : AbaScriptBaseVisitor<object?>
{
    private StringBuilder _stringBuilder = new();
    private int labelCounter = 0; // пока не понимаю зачем
    private Dictionary<string, int> functionLabels = new();
    private Dictionary<string, int> variableOffsets = new(); // Track variable offsets within functions
    private Stack<int> returnPoints = new(); // запоминает, куда возвращаться после вызова функции.
    private readonly IVariableStorage _variableStorage = new VariableStorage();

    public override object? VisitScript(AbaScriptParser.ScriptContext context)
    {
        _stringBuilder = new StringBuilder();

        foreach (var classDef in context.classDef())
        {
            Visit(classDef);
        }

        foreach (var funcDef in context.functionDef().Where(func => func.ID().GetText() != "main"))
        {
            Visit(funcDef);
        }
        
        var mainFunction = context.functionDef().FirstOrDefault(func => func.ID().GetText() == "main");
        if (mainFunction != null)
        {
            VisitMainFunction(mainFunction);
        }


        return _stringBuilder.ToString();
    }

    private object? VisitMainFunction(AbaScriptParser.FunctionDefContext context)
    {
        Visit(context.block());
        return null;
    }

    public string GetActualCompiledCode()
    {
        return _stringBuilder.ToString();
    }
}