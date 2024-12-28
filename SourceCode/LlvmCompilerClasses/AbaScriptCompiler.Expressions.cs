using LLVMSharp.Interop;

namespace AbaScript.LlvmCompilerClasses;

public partial class AbaScriptCompiler
{
    public override object VisitAddSub(AbaScriptParser.AddSubContext context)
    {
        Visit(context.expr());
        Visit(context.term());

        var right = _valueStack.Pop();
        var left = _valueStack.Pop();

        // Determine the operator by checking the text of the middle child
        var operatorText = context.GetChild(1).GetText();

        switch (right.TypeOf.Kind)
        {
            case LLVMTypeKind.LLVMIntegerTypeKind:
                if (left.TypeOf.Kind != LLVMTypeKind.LLVMIntegerTypeKind)
                {
                    throw new InvalidOperationException("Incompatible types.");
                }

                var rightValue = GetIntFromRef(right);
                var leftValue = GetIntFromRef(left);
                switch (operatorText)
                {
                    case "+":
                        _valueStack.Push(GetRefFromInt(leftValue + rightValue));
                        break;
                    case "-":
                        _valueStack.Push(GetRefFromInt(leftValue - rightValue));
                        break;
                    default:
                        throw new InvalidOperationException("Unsupported operation");
                }

                break;
            case LLVMTypeKind.LLVMArrayTypeKind:
                if (left.TypeOf.Kind != LLVMTypeKind.LLVMArrayTypeKind)
                {
                    throw new InvalidOperationException("Incompatible types.");
                }

                var leftAsString = GetStringFromRef(left);
                var rightAsString = GetStringFromRef(right);
                switch (operatorText)
                {
                    case "+":
                        _valueStack.Push(GetRefFromString(leftAsString + rightAsString));
                        break;
                    default:
                        throw new InvalidOperationException("Unsupported operation");
                }

                break;
        }

        _logger.Log($"left={left}, right={right}, leftType={left.TypeOf.Kind}, rightType={right.TypeOf.Kind}");
        return context;
    }
}