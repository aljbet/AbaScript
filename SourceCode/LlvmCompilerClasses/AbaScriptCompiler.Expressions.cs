using AbaScript.AntlrClasses;
using LLVMSharp.Interop;

namespace AbaScript.LlvmCompilerClasses;

// TODO: унарный минус
public partial class AbaScriptCompiler
{
    public override object VisitAddSub(AbaScriptParser.AddSubContext context)
    {
        // TODO: работает только для интов
        Visit(context.expr());
        Visit(context.term());

        var right = _valueStack.Pop();
        var left = _valueStack.Pop();
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

    public override object VisitAndExpr(AbaScriptParser.AndExprContext context)
    {
        Visit(context.logicalExpr());
        Visit(context.condition());
        var right = _valueStack.Pop();
        var left = _valueStack.Pop();
        _logger.Log($"left={left}, right={right}, leftType={left.TypeOf.Kind}, rightType={right.TypeOf.Kind}");
        _valueStack.Push(_builder.BuildAnd(left, right));
        return context;
    }

    public override object VisitOrExpr(AbaScriptParser.OrExprContext context)
    {
        Visit(context.logicalExpr());
        Visit(context.condition());
        var right = _valueStack.Pop();
        var left = _valueStack.Pop();
        _logger.Log($"left={left}, right={right}, leftType={left.TypeOf.Kind}, rightType={right.TypeOf.Kind}");
        _valueStack.Push(_builder.BuildAnd(left, right));
        return context;
    }

    public override object VisitConditionExpr(AbaScriptParser.ConditionExprContext context)
    {
        Visit(context.condition());
        return context;
    }

    public override object VisitCondition(AbaScriptParser.ConditionContext context)
    {
        // TODO: работает только для интов

        if (context.logicalExpr() != null)
        {
            Visit(context.logicalExpr());
            var logExpr = _valueStack.Pop();
            if (context.NOT() != null)
            {
                _valueStack.Push(_builder.BuildNot(logExpr));
            }
            else
            {
                _valueStack.Push(logExpr);
            }

            return context;
        }

        Visit(context.expr(0));
        Visit(context.expr(1));

        var right = _valueStack.Pop();
        var left = _valueStack.Pop();
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
                    case "==":
                        _valueStack.Push(_builder.BuildICmp(LLVMIntPredicate.LLVMIntEQ, left, right));
                        break;
                    case "!=":
                        _valueStack.Push(_builder.BuildICmp(LLVMIntPredicate.LLVMIntNE, left, right));
                        break;
                    case "<":
                        _valueStack.Push(_builder.BuildICmp(LLVMIntPredicate.LLVMIntSLT, left, right));
                        break;
                    case "<=":
                        _valueStack.Push(_builder.BuildICmp(LLVMIntPredicate.LLVMIntSLE, left, right));
                        break;
                    case ">":
                        _valueStack.Push(_builder.BuildICmp(LLVMIntPredicate.LLVMIntSLT, right, left));
                        break;
                    case ">=":
                        _valueStack.Push(_builder.BuildICmp(LLVMIntPredicate.LLVMIntSLE, right, left));
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

    public override object VisitBlock(AbaScriptParser.BlockContext context)
    {
        foreach (var statement in context.statement())
        {
            Visit(statement);
        }
        
        _valueStack.Push(_builder.InsertBlock.AsValue());

        return context;
    }

    private void ClearAfterReturn(LLVMBasicBlockRef block)
    {
        var instructionToDel = new List<LLVMValueRef>();
        bool hasReturn = false;
        for (var i = block.FirstInstruction; i != block.LastInstruction; i = i.NextInstruction)
        {
            if (hasReturn)
            {
                instructionToDel.Add(i);
            }
            else if (i.IsAReturnInst != null)
            {
                hasReturn = true;
            }
        }

        if (hasReturn)
        {
            instructionToDel.Add(block.LastInstruction);
        }

        for (var i = 0; i < instructionToDel.Count; i++)
        {
            instructionToDel[i].InstructionEraseFromParent();
        }
    }
}