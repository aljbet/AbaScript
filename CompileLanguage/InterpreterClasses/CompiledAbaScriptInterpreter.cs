using Antlr4.Runtime;
using CompileLanguage.BaseAntlrClasses;
using CompileLanguage.Exceptions;

namespace CompileLanguage.InterpreterClasses;

public partial class CompiledAbaScriptInterpreter : CompiledAbaScriptBaseVisitor<object?>
{
    private readonly Dictionary<string, int> _labels = new();
    private readonly Stack<int> _stack = new();
    private readonly Dictionary<string, int> _variables = new();
    private IList<CompiledAbaScriptParser.StatementContext> _statements;

    public void Interpret(string input)
    {
        var lexer = new CompiledAbaScriptLexer(new AntlrInputStream(input));
        var parser = new CompiledAbaScriptParser(new CommonTokenStream(lexer));
        _statements = parser.program().statement();
        for (var i = 0; i < _statements.Count; i++)
        {
            var stmt = _statements[i];
            if (stmt.label() != null)
            {
                var label = stmt.label().ID().GetText();
                if (_labels.ContainsKey(label))
                    throw new RuntimeException($"Label {label} already defined.");
                _labels[stmt.label().ID().GetText()] = i;
            }
        }

        ExecuteStatements();
    }

    private void ExecuteStatements()
    {
        for (var i = 0; i < _statements.Count; i++)
        {
            if (_statements.Count == 0)
            {
                break;
            }

            Visit(_statements[i]);

            if (_jumpDestination != -1)
            {
                i = _jumpDestination - 1;
                _jumpDestination = -1;
            }
        }
    }
}