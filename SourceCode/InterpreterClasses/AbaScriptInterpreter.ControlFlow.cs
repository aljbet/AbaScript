﻿using AbaScript.AntlrClasses;

namespace AbaScript.InterpreterClasses;

public partial class AbaScriptInterpreter
{
    public override object VisitIfStatement(AbaScriptParser.IfStatementContext context)
    {
        var oldVariables = CaptureCurrentScope();

        for (var i = 0; i < context.logicalExpr().Length; i++)
        {
            var conditionResult = (bool)Visit(context.logicalExpr(i));
            if (conditionResult)
            {
                Visit(context.block(i));
                RestoreScopeExcludingNewVariables(oldVariables);
                return null;
            }
        }

        if (context.block().Length > context.logicalExpr().Length)
        {
            Visit(context.block(context.logicalExpr().Length));
        }

        RestoreScopeExcludingNewVariables(oldVariables);
        return null;
    }

    public override object VisitWhileStatement(AbaScriptParser.WhileStatementContext context)
    {
        var oldVariables = CaptureCurrentScope();
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

        RestoreScopeExcludingNewVariables(oldVariables);
        return null;
    }

    public override object VisitForStatement(AbaScriptParser.ForStatementContext context)
    {
        var oldVariables = CaptureCurrentScope();
        var assignments = context.assignment().ToList();

        if (context.variableDeclaration() != null)
        {
            Visit(context.variableDeclaration());
        }
        else if (assignments[0] != null)
        {
            Visit(context.assignment(0));
        }

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
                if (context.assignment(1) != null)
                {
                    Visit(context.assignment(1));
                }

                continue;
            }

            if (assignments.Count > 1)
            {
                Visit(assignments[1]);
            }
            else
            {
                if (assignments.Count == 1 && context.variableDeclaration() != null)
                {
                    Visit(assignments[0]);
                }
            }
        }

        RestoreScopeExcludingNewVariables(oldVariables);
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