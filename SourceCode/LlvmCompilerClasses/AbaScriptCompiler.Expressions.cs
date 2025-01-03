using LLVMSharp.Interop;

namespace AbaScript.LlvmCompilerClasses;

public partial class AbaScriptCompiler
{
    public override object VisitAddSub(AbaScriptParser.AddSubContext context)
    {
        // TODO: работает только для интов
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
                
                switch (operatorText)
                {
                    case "+":
                        _valueStack.Push(_builder.BuildAdd(left, right));
                        break;
                    case "-":
                        _valueStack.Push(_builder.BuildSub(left, right));
                        break;
                    default:
                        throw new InvalidOperationException("Unsupported operation");
                }

                break;
            // case LLVMTypeKind.LLVMArrayTypeKind:
            //     if (left.TypeOf.Kind != LLVMTypeKind.LLVMArrayTypeKind)
            //     {
            //         throw new InvalidOperationException("Incompatible types.");
            //     }
            //
            //     var leftAsString = GetStringFromRef(left);
            //     var rightAsString = GetStringFromRef(right);
            //     switch (operatorText)
            //     {
            //         case "+":
            //             _valueStack.Push(GetRefFromString(leftAsString + rightAsString));
            //             break;
            //         default:
            //             throw new InvalidOperationException("Unsupported operation");
            //     }
            //
            //     break;
            default:
                throw new InvalidOperationException("Unsupported types");
        }

        _logger.Log($"left={left}, right={right}, leftType={left.TypeOf.Kind}, rightType={right.TypeOf.Kind}");
        return context;
    }

    public override object VisitMulDivMod(AbaScriptParser.MulDivModContext context)
    {
        // TODO: работает только для интов

        Visit(context.term());
        Visit(context.factor());
        
        var right = _valueStack.Pop();
        var left = _valueStack.Pop();

        // Determine the operator by checking the text of the middle child
        var operatorText = context.GetChild(1).GetText();

        _logger.Log($"left={left}, right={right}, leftType={left.TypeOf.Kind}, rightType={right.TypeOf.Kind}");

        switch (right.TypeOf.Kind)
        {
            case LLVMTypeKind.LLVMIntegerTypeKind:
                if (left.TypeOf.Kind != LLVMTypeKind.LLVMIntegerTypeKind)
                {
                    throw new InvalidOperationException("Incompatible types.");
                }

                switch (operatorText)
                {
                    case "*":
                        _valueStack.Push(_builder.BuildMul(left, right));
                        break;
                    case "/":
                        _valueStack.Push(_builder.BuildSDiv(left, right));
                        break;
                    case "%":
                        _valueStack.Push(_builder.BuildSRem(left, right));
                        break;
                    default:
                        throw new InvalidOperationException("Unsupported operation");
                }

                break;
            default:
                throw new InvalidOperationException("Unsupported types");
        }

        return context;
    }
}