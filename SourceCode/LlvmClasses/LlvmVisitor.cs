using System.Runtime.InteropServices;
using LLVMSharp;
using LLVMSharp.Interop;

namespace AbaScript.LlvmClasses;

public class LlvmVisitor : AbaScriptBaseVisitor<object>
{
    private readonly Logger logger = new();
    private readonly Stack<LLVMValueRef> valueStack = new();
    private readonly Dictionary<string, object> variables = new();
    
    public override unsafe object VisitOutputStatement(AbaScriptParser.OutputStatementContext context)
    {
        var value = Visit(context.expr());
        Console.WriteLine(LLVM.ConstIntGetSExtValue((LLVMOpaqueValue*) valueStack.Pop().Handle)); // 6
        return context;
    }

    public override unsafe object VisitNumber(AbaScriptParser.NumberContext context)
    {
        if (ulong.TryParse(context.GetText(), out var number))
        {
            valueStack.Push(LLVM.ConstInt(LLVM.IntType(32), number, 1));
            return context;
        }
        throw new InvalidOperationException($"Невозможно преобразовать в число: {context.GetText()}");
    }
}