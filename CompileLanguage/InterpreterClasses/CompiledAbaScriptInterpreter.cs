using Antlr4.Runtime;
using CompileLanguage.BaseAntlrClasses;
using CompileLanguage.Exceptions;

namespace CompileLanguage.InterpreterClasses;

// где храним переменную
public enum Storage
{
    Heap,
    Stack
}

public struct Variable
{
    public Variable(string type, int address, Storage storage, string name)
    {
        Type = type;
        Address = address;
        Storage = storage;
        Name = name;
    }
    
    public string Type;
    public int Address;
    public Storage Storage;
    public string Name;
}

// этот контекст можно будет заранее обработать
public struct ClassInfo
{
    public Variable[] Fields;
    
    public ClassInfo(Variable[] fields)
    {
        Fields = fields;
    }
}

public struct SimpleTypes
{
    public static String boolType = "bool";
    public static String intType = "int";
    public static String stringType = "string";

    public static bool IsSimple(String t)
    {
        return t == boolType || t == intType || t == stringType;
    }
}

public partial class CompiledAbaScriptInterpreter : CompiledAbaScriptBaseVisitor<object?>
{
    private readonly Dictionary<string, int> _labels = new();
    private readonly Stack<int> _stack = new();
    private readonly Dictionary<string, Variable> _variables = new(); // храним информацию о переменных в стеке (инстансы классов это указатели на кучу)
    private IList<CompiledAbaScriptParser.StatementContext> _statements;
    private Dictionary<int, int> _heapAddresses = new(); // адресное пространство кучи
    Dictionary<int, int> _stackAddresses = new();        // адресное пространство стека
    Dictionary<int, int> _linkCounter = new();           // счётчик ссылок
    private Stack<int> _scopeStack = new();
    private int _stackTop = 0;
    private int _heapTop = 0;
    
    private Dictionary<String, ClassInfo> _classInfos = new();

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
            Visit(_statements[i]);

            if (_jumpDestination != -1)
            {
                i = _jumpDestination - 1;
                _jumpDestination = -1;
            }
        }
    }
}