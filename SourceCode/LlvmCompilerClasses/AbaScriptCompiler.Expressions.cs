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

    public override object VisitIfStatement(AbaScriptParser.IfStatementContext context)
    {
        // TODO: elif
        // TODO: and, or
        // TODO: return inside if
        Visit(context.logicalExpr());
        var condition = _valueStack.Pop();
        var condv = _builder.BuildICmp(LLVMIntPredicate.LLVMIntNE, condition,
            LLVMValueRef.CreateConstInt(_context.GetIntType(1), 0), "ifcond");
        var func = _builder.InsertBlock.Parent;
        var thenBB = LLVMBasicBlockRef.AppendInContext(_context, func, "then");
        var elseBB = LLVMBasicBlockRef.AppendInContext(_context, func, "else");
        var mergeBB = LLVMBasicBlockRef.AppendInContext(_context, func, "merge");
        _builder.BuildCondBr(condv, thenBB, elseBB);

        _builder.PositionAtEnd(thenBB);
        Visit(context.block(0));
        var then_block = _valueStack.Pop();
        _builder.BuildBr(mergeBB);

        ClearAfterReturn(thenBB);
        thenBB = _builder.InsertBlock;

        _builder.PositionAtEnd(elseBB);
        Visit(context.block(1));
        var else_block = _valueStack.Pop();
        _builder.BuildBr(mergeBB);
        ClearAfterReturn(elseBB);
        elseBB = _builder.InsertBlock;

        _builder.PositionAtEnd(mergeBB);
        // var phi = _builder.BuildPhi(_context.GetIntType(32), "phi");
        // phi.AddIncoming(new[] { then_block }, new[] { thenBB }, 1);
        // phi.AddIncoming(new[] { else_block }, new[] { elseBB }, 1);
        // _valueStack.Push(phi);
        return context;
    }

    public override object VisitLogicalExpr(AbaScriptParser.LogicalExprContext context)
    {
        if (context.children.Count == 1)
        {
            Visit(context.GetChild(0));
            return context;
        }

        Visit(context.GetChild(0));
        Visit(context.GetChild(2));

        var right = _valueStack.Pop();
        var left = _valueStack.Pop();
        var operatorText = context.GetChild(1).GetText();

        _logger.Log($"left={left}, right={right}, leftType={left.TypeOf.Kind}, rightType={right.TypeOf.Kind}");

        switch (operatorText)
        {
            case "&&":
                _valueStack.Push(_builder.BuildAnd(left, right));
                break;
            case "||":
                _valueStack.Push(_builder.BuildOr(left, right));
                break;
            default:
                throw new InvalidOperationException("Unsupported operation");
        }

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
            } else if (i.IsAReturnInst != null)
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