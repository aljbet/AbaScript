using LLVMSharp;
using LLVMSharp.Interop;

namespace AbaScript.LLVM;

public class LlvmVisitor : AbaScriptBaseVisitor<object>
{
    private readonly Stack<LLVMValueRef> valueStack = new();
    private readonly Logger logger = new();

    public override object VisitVariableDeclaration(AbaScriptParser.VariableDeclarationContext context)
    {
        var varName = context.ID().GetText();
        object value = null;

        if (context.NUMBER() != null)
        {
            // Array declaration with a specified size
            int size = int.Parse(context.NUMBER().GetText());
            value = new object[size];
        }
        else if (context.expr() != null)
        {
            // Regular variable initialization
            value = Visit(context.expr());
        }
        
        this.valueStack.Push(new LLVMValueRef());
        variables[varName] = value;
        logger.Log($"Переменная {varName} объявлена со значением: {value} (тип: {GetType(value)})");
        return null;
    }
    
    public override object VisitNumber(AbaScriptParser.NumberContext context)
    {
        // this.valueStack.Push(LLVM.ConstReal(LLVM.DoubleType(), node.Value));
        // return node;
        if (int.TryParse(context.GetText(), out var number))
        {
            valueStack.Push(LLVM.ConstReal(LL)); LLVMSharp.ConstantFP(LLVMSharp.SequentialType);
        }
        throw new InvalidOperationException($"Невозможно преобразовать в число: {context.GetText()}");
    }
}