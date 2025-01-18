namespace CompileLanguage.InterpreterClasses;

public partial class CompiledAbaScriptInterpreter : CompiledAbaScriptBaseVisitor<object?>
{
    private readonly Dictionary<string, int> _labels = new();
    private readonly Stack<int> _stack = new();
    private readonly Dictionary<string, int> _variables = new();
    private IList<CompiledAbaScriptParser.StatementContext> _statements;

    public void Interpret(CompiledAbaScriptParser.ProgramContext program)
    {
        _statements = program.statement();
        for (var i = 0; i < _statements.Count; i++)
        {
            var stmt = _statements[i];
            if (stmt.label() != null) _labels[stmt.label().ID().GetText()] = i;
        }

        ExecuteStatements();
    }

    private void ExecuteStatements()
    {
        foreach (var t in _statements) Visit(t);
    }
}