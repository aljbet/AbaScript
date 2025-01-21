﻿using Antlr4.Runtime;
using CompileLanguage.BaseAntlrClasses;
using CompileLanguage.Exceptions;

namespace CompileLanguage.InterpreterClasses;

public partial class CompiledAbaScriptInterpreter : CompiledAbaScriptBaseVisitor<object?>
{
    private readonly Dictionary<string, int> _labels = new();
    private readonly Stack<int> _stack = new();
    private readonly Dictionary<string, Stack<Variable>> _variables = new(); // храним информацию о переменных в стеке (инстансы классов это указатели на кучу)
    private readonly Stack<int> _functionCalls = new();
    private IList<CompiledAbaScriptParser.StatementContext> _statements;
    private Dictionary<int, int> _heapAddresses = new(); // адресное пространство кучи
    Dictionary<int, int> _stackAddresses = new();        // адресное пространство стека
    Dictionary<int, int> _linkCounter = new();           // счётчик ссылок
    private Stack<int> _scopeStack = new();
    private int _stackTop;
    private int _heapTop;
    private int _commandPos;
    private int _startPos;
    
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
            Visit(_statements[_commandPos]);

            if (_jumpDestination != -1)
            {
                _commandPos = _jumpDestination - 1;
                _jumpDestination = -1;
            }
        }
    }
}