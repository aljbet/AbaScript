using AbaScript.AntlrClasses;
using LLVMSharp.Interop;

namespace AbaScript.LlvmCompilerClasses;

public partial class AbaScriptCompiler
{
    public override object VisitIfStatement(AbaScriptParser.IfStatementContext context)
    {
        // TODO: elif
        Visit(context.logicalExpr(0));
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
        _builder.BuildBr(mergeBB);
        ClearAfterReturn(thenBB);

        _builder.PositionAtEnd(elseBB);
        Visit(context.block(1));
        _builder.BuildBr(mergeBB);
        ClearAfterReturn(elseBB);

        _builder.PositionAtEnd(mergeBB);
        return context;
    }

    public override object VisitForStatement(AbaScriptParser.ForStatementContext context)
    {
        if (context.variableDeclaration() != null)
        {
            Visit(context.variableDeclaration());
        }
        else
        {
            Visit(context.assignment(0));
        }

        var preheaderBB = _builder.InsertBlock;
        var func = preheaderBB.Parent;
        var loopBB = LLVMBasicBlockRef.AppendInContext(_context, func, "loop");
        _builder.BuildBr(loopBB);
        _builder.PositionAtEnd(loopBB);

        Visit(context.block());
        if (context.variableDeclaration() != null)
        {
            Visit(context.assignment(0));
        }
        else
        {
            Visit(context.assignment(1));
        }

        Visit(context.logicalExpr());
        var logExprValue = _valueStack.Pop();

        var afterBB = LLVMBasicBlockRef.AppendInContext(_context, func, "afterloop");

        _builder.BuildCondBr(logExprValue, loopBB, afterBB);
        _builder.PositionAtEnd(afterBB);

        _valueStack.Push(LLVMValueRef.CreateConstInt(_context.GetIntType(1), 0));

        return context;
    }
}