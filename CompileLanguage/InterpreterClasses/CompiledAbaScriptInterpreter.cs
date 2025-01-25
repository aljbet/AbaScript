using System.Text.RegularExpressions;
using Antlr4.Runtime;
using CompileLanguage.BaseAntlrClasses;
using CompileLanguage.Exceptions;

namespace CompileLanguage.InterpreterClasses;

public partial class CompiledAbaScriptInterpreter : CompiledAbaScriptBaseVisitor<object?>
{
    private Dictionary<string, int> _labels = new();
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
    private int _uniqueIndex;
    private Dictionary<string, BlockInfo> _optimizationCache = new();

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

        var block = new BlockInfo(_labels, _statements, input.Split("\r\n"));
        RunBlock(block, true, _startPos);
    }

    private void RunBlock(BlockInfo block, bool isJitNeeded, int startPos=0)
    {
        var oldLabels = _labels;
        _labels = block.Labels;
        for (var i = startPos; i < block.Statements.Count; i++)
        {
            _commandPos = i;
            if (isJitNeeded)
            {
                if (block.Statements[i].label() != null)
                {
                    var curLabel = _statements[i].label().GetText();
                    if (Regex.IsMatch(curLabel, Keywords.FOR_LABEL)) // возможно можем оптимизировать
                    {
                        var numberFor = curLabel.Substring(Keywords.FOR_LABEL.Length,
                            curLabel.Length - Keywords.FOR_LABEL.Length - 1);
                        
                        if (_optimizationCache.ContainsKey(curLabel))
                        {
                            RunBlock(_optimizationCache[curLabel], false);
                            i = block.Labels[Keywords.FOR_END_LABEL + numberFor];
                            continue;
                        }
                        if (IsPossibleToUnroll(numberFor, block))
                        {
                            var jitBlock = Unroll(numberFor, block);
                            _optimizationCache.Add(curLabel, jitBlock);

                            RunBlock(jitBlock, false);
                            i = block.Labels[Keywords.FOR_END_LABEL + numberFor];
                            continue;
                        }
                    }
                }
            }

            Visit(block.Statements[i]);
            if (_jumpDestination != -1)
            {
                i = _jumpDestination - 1;
                _jumpDestination = -1;
            }
        }
        _labels = oldLabels;
    }
}