using AbaScript.AntlrClasses;
using LLVMSharp.Interop;

namespace AbaScript.LlvmCompilerClasses;

public partial class AbaScriptCompiler
{
    public override object VisitIfStatement(AbaScriptParser.IfStatementContext context)
    {
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
        var currFunc = _builder.InsertBlock.Parent;
        var loopCondBB = _context.AppendBasicBlock(currFunc, "forCond");
        var loopBodyBB = _context.AppendBasicBlock(currFunc, "forBody");
        var loopIncBB = _context.AppendBasicBlock(currFunc, "forInc");
        var loopAfterBB = _context.AppendBasicBlock(currFunc, "afterFor");
        
        if (context.variableDeclaration() != null)
        {
            Visit(context.variableDeclaration());
        }
        else
        {
            Visit(context.assignment(0));
        }
        _builder.BuildBr(loopCondBB);

        _builder.PositionAtEnd(loopCondBB);
        Visit(context.logicalExpr());
        var logExprValue = _valueStack.Pop();
        _builder.BuildCondBr(logExprValue, loopBodyBB, loopAfterBB);

        _builder.PositionAtEnd(loopBodyBB);
        Visit(context.block());
        _builder.BuildBr(loopIncBB);
        
        _builder.PositionAtEnd(loopIncBB);
        if (context.variableDeclaration() != null)
        {
            Visit(context.assignment(0));
        }
        else
        {
            Visit(context.assignment(1));
        }
        _builder.BuildBr(loopCondBB);

        _builder.PositionAtEnd(loopAfterBB);

        return context;
    }
}