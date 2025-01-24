using System.Text.RegularExpressions;
using Antlr4.Runtime;
using CompileLanguage.BaseAntlrClasses;
using CompileLanguage.Exceptions;

namespace CompileLanguage.InterpreterClasses;

public partial class CompiledAbaScriptInterpreter : CompiledAbaScriptBaseVisitor<object?>
{
    private readonly Dictionary<string, int> _labels = new();
    private readonly Stack<long> _stack = new();

    private readonly Dictionary<string, Stack<Variable>>
        _variables = new(); // храним информацию о переменных в стеке (инстансы классов это указатели на кучу)

    private readonly Stack<int> _functionCalls = new();
    private IList<CompiledAbaScriptParser.StatementContext> _statements;
    private Dictionary<long, long> _heapAddresses = new(); // адресное пространство кучи
    Dictionary<long, long> _stackAddresses = new(); // адресное пространство стека
    Dictionary<long, long> _linkCounter = new(); // счётчик ссылок
    private Stack<int> _scopeStack = new();
    private int _stackTop;
    private int _heapTop;
    private int _commandPos;
    private int _startPos;
    private Dictionary<string, IList<CompiledAbaScriptParser.StatementContext>> _optimizationCache = new();

    /* private надо где-то хранить функции, к которым потом применится оптимизация.
     в каком виде? участок колбасы
     что такое участок колбасы? две ссылки: нода с которой все начинается (enter_scope) и которой все заканчивается (exit)
     они остаются, а то что между ними можно поменять.
     сначала придумываем, как поменять, потом возвращаемся в начало и исполняем

     чтобы создать участок колбасы надо создать полноценный узел контекста.
     надо понять, что вообще входит в узел :(

    */

    private Dictionary<String, ClassInfo> _classInfos;

    public CompiledAbaScriptInterpreter(Dictionary<String, ClassInfo> classInfos)
    {
        _classInfos = classInfos;
    }

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
                if (label == "main")
                {
                    _startPos = i;
                    _functionCalls.Push(_statements.Count);
                }

                _labels[stmt.label().ID().GetText()] = i;
            }
        }

        ExecuteStatements();
    }

    private void ExecuteStatements()
    {
        for (_commandPos = _startPos; _commandPos < _statements.Count; _commandPos++)
        {
            if (_statements[_commandPos].label() != null)
            {
                // возможно нужна оптимизация
                var curLabel = _statements[_commandPos].label().GetText();
                if (Regex.IsMatch(curLabel, Keywords.FOR_LABEL) &&
                    _statements[_commandPos - 2].GetText() == Keywords.LT)
                {
                    LoopUnroll(curLabel);
                }
                // else if ()
                // {
                    
                // }
            }

            Visit(_statements[_commandPos]);
            if (_jumpDestination != -1)
            {
                _commandPos = _jumpDestination - 1;
                _jumpDestination = -1;
            }
        }
    }
}