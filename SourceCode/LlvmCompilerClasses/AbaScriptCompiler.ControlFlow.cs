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

        var startValue = _valueStack.Pop();

        var preheaderBB = _builder.InsertBlock;
        var func = preheaderBB.Parent;
        var loopBB =  LLVMBasicBlockRef.AppendInContext(_context, func, "loop");
        _builder.BuildBr(loopBB);
        _builder.PositionAtEnd(loopBB);
        
        // var phi = _builder.BuildPhi(_context.GetIntType(32), "phi");
        // phi.AddIncoming(new []{startValue}, new []{preheaderBB}, 1);

        Visit(context.block());
        if (context.variableDeclaration() != null)
        {
            Visit(context.assignment(0));
        }
        else
        {
            Visit(context.assignment(1));
        }
        var nextValue = _valueStack.Pop();
        Visit(context.logicalExpr());
        var logExprValue = _valueStack.Pop();

        var loopEndBB = _builder.InsertBlock;
        var afterBB =  LLVMBasicBlockRef.AppendInContext(_context, func, "afterloop");

        _builder.BuildCondBr(logExprValue, loopBB, afterBB);
        _builder.PositionAtEnd(afterBB);
        
        // phi.AddIncoming(new []{nextValue}, new []{loopEndBB}, 1);
        _valueStack.Push(LLVMValueRef.CreateConstInt(_context.GetIntType(1), 0));
        return context;
    }
}