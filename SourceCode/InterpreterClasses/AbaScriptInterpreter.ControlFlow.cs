using AbaScript.AntlrClasses;

namespace AbaScript.InterpreterClasses;

public partial class AbaScriptInterpreter
{
    public override object VisitIfStatement(AbaScriptParser.IfStatementContext context)
    {
        for (var i = 0; i < context.logicalExpr().Length; i++)
        {
            var conditionResult = (bool)Visit(context.logicalExpr(i));
            if (conditionResult)
            {
                Visit(context.block(i));
                return null;
            }
        }

        if (context.block().Length > context.logicalExpr().Length)
            // Это блок else
            Visit(context.block(context.logicalExpr().Length));

        return null;
    }

    public override object VisitWhileStatement(AbaScriptParser.WhileStatementContext context)
    {
        while (true)
        {
            var conditionResult = Visit(context.logicalExpr());
            if (conditionResult is not bool boolResult)
                throw new InvalidOperationException("The condition must evaluate to a boolean value.");

            if (!boolResult)
                break;

            try
            {
                Visit(context.block());
            }
            catch (BreakException)
            {
                break;
            }
            catch (ContinueException) { }
        }

        return null;
    }

    public override object VisitForStatement(AbaScriptParser.ForStatementContext context)
    {
        // Инициализация
        if (context.variableDeclaration() != null)
            Visit(context.variableDeclaration());
        else if (context.assignment(0) != null) Visit(context.assignment(0));

        while (true)
        {
            if (context.logicalExpr() != null)
            {
                var conditionResult = Visit(context.logicalExpr());
                if (conditionResult is not bool boolResult)
                    throw new InvalidOperationException("The condition must evaluate to a boolean value.");

                if (!boolResult)
                    break;
            }

            try
            {
                Visit(context.block());
            }
            catch (BreakException)
            {
                break;
            }
            catch (ContinueException)
            {
                // Выполняем инкремент перед продолжением
                if (context.assignment(1) != null)
                    Visit(context.assignment(1));
                continue;
            }

            // Инкремент
            if (context.assignment(1) != null)
                Visit(context.assignment(1));
        }

        return null;
    }

    public override object VisitBreakStatement(AbaScriptParser.BreakStatementContext context)
    {
        throw new BreakException();
    }

    public override object VisitContinueStatement(AbaScriptParser.ContinueStatementContext context)
    {
        throw new ContinueException();
    }
}