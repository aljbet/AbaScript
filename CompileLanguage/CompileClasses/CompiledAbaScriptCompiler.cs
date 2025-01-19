using System.Text;
using AbaScript.AntlrClasses;

namespace CompileLanguage.CompileClasses;

public partial class CompiledAbaScriptCompiler : AbaScriptBaseVisitor<object?>
{
    private StringBuilder _stringBuilder = new();
    private int labelCounter = 0; // пока не понимаю зачем
    private Dictionary<string, int> functionLabels = new();
    private Dictionary<string, int> variableOffsets = new(); // Track variable offsets within functions
    private Stack<int> returnPoints = new(); // запоминает, куда возвращаться после вызова функции.

    public override object? VisitScript(AbaScriptParser.ScriptContext context)
    {
        _stringBuilder = new StringBuilder();
        foreach (var funcDef in context.functionDef())
        {
            Visit(funcDef);
        }

        foreach (var classDef in context.classDef())
        {
            Visit(classDef);
        }

        return _stringBuilder.ToString();
    }
}