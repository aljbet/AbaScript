﻿using LLVMSharp.Interop;

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

    public override object VisitIfStatement(AbaScriptParser.IfStatementContext context)
    {
        Visit(context.logicalExpr());
        var condition = _valueStack.Pop();
        var condv = _builder.BuildICmp(LLVMIntPredicate.LLVMIntNE, condition,
            LLVMValueRef.CreateConstInt(_context.Int32Type, 0), "ifcond");
        var func = _builder.InsertBlock.Parent;
        var thenBB = LLVMBasicBlockRef.AppendInContext(_context, func, "then");
        var elseBB = LLVMBasicBlockRef.AppendInContext(_context, func, "else");
        var mergeBB = LLVMBasicBlockRef.AppendInContext(_context, func, "merge");
        _builder.BuildCondBr(condv, thenBB, elseBB);

        _builder.PositionAtEnd(thenBB);
        Visit(context.block(0));
        var then_block = _valueStack.Pop();
        _builder.BuildBr(mergeBB);
        thenBB = _builder.InsertBlock;

        _builder.PositionAtEnd(elseBB);
        Visit(context.block(1));
        var else_block = _valueStack.Pop();
        _builder.BuildBr(mergeBB);
        elseBB = _builder.InsertBlock;

        _builder.PositionAtEnd(mergeBB);
        var phi = _builder.BuildPhi(_context.GetIntType(1), "phi");
        phi.AddIncoming(new[] { then_block }, new[] { thenBB }, 1);
        phi.AddIncoming(new[] { else_block }, new[] { elseBB }, 1);
        _valueStack.Push(phi);
        return context;
    }

    public override object VisitCondition(AbaScriptParser.ConditionContext context)
    {
        // TODO: работает только для интов

        Visit(context.expr(0));
        Visit(context.expr(1));

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
//             "==" => Equals(left, right),
//             "!=" => !Equals(left, right),
//             "<" => (int)left < (int)right,
//             "<=" => (int)left <= (int)right,
//             ">" => (int)left > (int)right,
//             ">=" => (int)left >= (int)right,
//             _ => throw new InvalidOperationException("Unsupported comparison operation")
                    case "==":
                        var a = _builder.BuildICmp(LLVMIntPredicate.LLVMIntEQ, left, right);
                        // _valueStack.Push(_builder.BuildUIToFP(a, LLVMTypeRef.Double));
                        _valueStack.Push(a);
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
}