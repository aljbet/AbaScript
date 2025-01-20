using System.Text;
using AbaScript.AntlrClasses;
using AbaScript.AntlrClasses.Models;
using LLVMSharp.Interop;

namespace AbaScript.LlvmCompilerClasses;

public partial class AbaScriptCompiler : AbaScriptBaseVisitor<object>
{
    private readonly Logger _logger = new();
    private readonly Stack<LLVMValueRef> _valueStack = new();
    private readonly ScopeManager<AllocaInfo> _scopeManager = new();
    private readonly Dictionary<string, LLVMTypeRef> _funcTypes = new();
    private readonly LLVMTypeRef _intType;

    private LLVMModuleRef _module;
    private LLVMBuilderRef _builder;
    private LLVMContextRef _context;

    public AbaScriptCompiler(LLVMContextRef context, LLVMModuleRef module, LLVMBuilderRef builder)
    {
        _context = context;
        _module = module;
        _builder = builder;
        _intType = _context.GetIntType(64);
    }

    public override object VisitScript(AbaScriptParser.ScriptContext context)
    {
        GetOrCreatePrintFunc();

        VisitChildren(context);

        return context;
    }

    private LLVMValueRef GetRefFromInt(long value)
    {
        return LLVMValueRef.CreateConstInt(_intType, (ulong)value);
    }

    private static bool CheckType(string type, LLVMTypeKind valueType)
    {
        return Enum.TryParse(type, true, out VariableType variableType) && variableType switch
        {
            VariableType.Int => valueType is LLVMTypeKind.LLVMIntegerTypeKind,
            _ => false,
        };
    }

    private LLVMTypeRef TypeMatch(string type)
    {
        return type switch
        {
            "int" => _intType,
            _ => throw new InvalidOperationException($"Неизвестный тип {type}")
        };
    }
}