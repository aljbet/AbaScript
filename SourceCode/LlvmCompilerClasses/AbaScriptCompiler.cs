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
        _intType = _context.GetIntType(32);
    }

    public override object VisitScript(AbaScriptParser.ScriptContext context)
    {
        GetOrCreatePrintFunc();

        VisitChildren(context);

        return context;
    }

    private unsafe long GetIntFromRef(LLVMValueRef valueRef)
    {
        return LLVM.ConstIntGetSExtValue((LLVMOpaqueValue*)valueRef.Handle);
    }

    private string GetStringFromRef(LLVMValueRef valueRef)
    {
        var asString = valueRef.ToString();
        var index = asString.IndexOf(']');
        return asString.Substring(index + 4, asString.Length - index - 8);
    }

    private LLVMValueRef GetRefFromInt(long value)
    {
        return LLVMValueRef.CreateConstInt(_intType, (ulong)value);
    }

    private unsafe LLVMValueRef GetRefFromString(string str)
    {
        var bytes = Encoding.Default.GetBytes(str);

        fixed (byte* p = bytes)
        {
            sbyte* sp = (sbyte*)p;
            return LLVM.ConstString(sp, (uint)str.Length, 0);
        }
    }

    // Return as string if not a number
    private object? TryParseNumber(string? input)
    {
        if (int.TryParse(input, out var number))
        {
            return number;
        }

        return input;
    }
    private static bool CheckType(string type, LLVMTypeKind valueType)
    {
        return Enum.TryParse(type, true, out VariableType variableType) && variableType switch
        {
            VariableType.Int => valueType is LLVMTypeKind.LLVMIntegerTypeKind,
            VariableType.String => valueType is LLVMTypeKind.LLVMArrayTypeKind,
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