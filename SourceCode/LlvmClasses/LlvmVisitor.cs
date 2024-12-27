using System.Text;
using LLVMSharp.Interop;

namespace AbaScript.LlvmClasses;

public class LlvmVisitor : AbaScriptBaseVisitor<object>
{
    private readonly Logger logger = new();
    private readonly Stack<LLVMValueRef> valueStack = new();
    private readonly Dictionary<string, object> variables = new();

    public override unsafe object VisitOutputStatement(AbaScriptParser.OutputStatementContext context)
    {
        Visit(context.expr());
        var currentElement = valueStack.Pop();

        switch (currentElement.TypeOf.Kind)
        {
            case LLVMTypeKind.LLVMIntegerTypeKind:
                Console.WriteLine(LLVM.ConstIntGetSExtValue((LLVMOpaqueValue*)currentElement.Handle));
                break;
            case LLVMTypeKind.LLVMArrayTypeKind:
                var asString = currentElement.ToString();
                var index = asString.IndexOf(']');
                Console.WriteLine(asString.Substring(index + 4, asString.Length - index - 8));
                break;
        }

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

    public override unsafe object VisitString(AbaScriptParser.StringContext context)
    {
        var str = context.GetText().Trim('"');
        var bytes = Encoding.Default.GetBytes(str);

        fixed (byte* p = bytes)
        {
            sbyte* sp = (sbyte*)p;
            valueStack.Push(LLVM.ConstString(sp, (uint)str.Length, 0));
        }

        return context;
    }
}