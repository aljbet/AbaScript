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
            // if надо изменить порядок (добавить оптимизацию) то меняем, иначе вот это
            // в каких случаях нужна оптимизациия?
            if (_statements[_commandPos].label() != null)
            {
                // возможно нужна оптимизация
                var curLabel = _statements[_commandPos].label().GetText();
                if (Regex.IsMatch(curLabel, Keywords.FOR_LABEL) &&
                    _statements[_commandPos - 2].GetText() == Keywords.LT)
                {
                    // узнаем число n
                    // команда LOAD что то на 3 выше for_label_
                    Visit(_statements[_commandPos - 4]);
                    Visit(_statements[_commandPos - 3]);
                    var n = _stack.Pop() - _stack.Pop();
                    // number = на что заканчивается label
                    var numberFor = curLabel.Substring(Keywords.FOR_LABEL.Length,
                        curLabel.Length - Keywords.FOR_LABEL.Length - 1);
                    if (_statements[_labels[Keywords.FOR_END_LABEL + numberFor] - 4].GetText() !=
                        Keywords.PUSH + " 1")
                        continue;

                    // считываем все до JMP for_logic_label_{number} и запоминаем в особую структуру (массив или одна строка).
                    var posInForLoop = _commandPos + 1;
                    var statementsInForLoop = new List<CompiledAbaScriptParser.StatementContext>();
                    while (_statements[posInForLoop].GetText() != Keywords.JMP + Keywords.FOR_LOGIC_LABEL + numberFor)
                    {
                        statementsInForLoop.Add(_statements[posInForLoop]);
                        posInForLoop++;
                    }

                    // n раз визитим все из этой структуры.
                    for (int i = 0; i < n; i++)
                    {
                        for (int index__ = 0; index__ < statementsInForLoop.Count; index__++)
                        {
                            Visit(statementsInForLoop[index__]);
                            if (_jumpDestination != -1)
                            {
                                index__ = _jumpDestination - 1 - _commandPos;
                                _jumpDestination = -1;
                            }
                        }
                    }

                    _commandPos = posInForLoop + 1;
                    continue;
                }
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