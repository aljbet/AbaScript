using System.Text;
using AbaScript.AntlrClasses;

namespace CompileLanguage.CompileClasses;

public partial class CompiledAbaScriptCompiler : AbaScriptBaseVisitor<object?>
{
    private StringBuilder _stringBuilder = new StringBuilder();
    private int labelCounter = 0;
    private Dictionary<string, int> functionLabels = new Dictionary<string, int>();
    private Dictionary<string, int> variableOffsets = new Dictionary<string, int>(); // Track variable offsets within functions

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

        foreach (var statement in context.statement())
        {
            Visit(statement);
        }

        return _stringBuilder.ToString();
    }
}