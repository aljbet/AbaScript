using AbaScript.AntlrClasses;

namespace CompileLanguage.CompileClasses;

public partial class CompiledAbaScriptCompiler
{
    public override object VisitAddSub(AbaScriptParser.AddSubContext context)
    {
        Visit(context.expr());
        Visit(context.term());
        switch (context.GetChild(1).GetText())
        {
            case "+":
                _stringBuilder.AppendLine(Keywords.ADD);
                break;
            case "-":
                _stringBuilder.AppendLine(Keywords.SUB);
                break;
            default:
                throw new InvalidOperationException("Unsupported operation");
        }

        return context;
    }

    public override object VisitMulDivMod(AbaScriptParser.MulDivModContext context)
    {
        Visit(context.term());
        Visit(context.factor());
        switch (context.GetChild(1).GetText())
        {
            case "*":
                _stringBuilder.AppendLine(Keywords.MUL);
                break;
            case "/":
                _stringBuilder.AppendLine(Keywords.DIV);
                break;
            case "%":
                _stringBuilder.AppendLine(Keywords.MOD);
                break;
            default:
                throw new InvalidOperationException("Unsupported operation");
        }

        return context;
    }

    public override object VisitLogicalExpr(AbaScriptParser.LogicalExprContext context)
    {
        switch (context)
        {
            case AbaScriptParser.AndExprContext andExpr:
            {
                Visit(andExpr.logicalExpr());
                Visit(andExpr.condition());
                _stringBuilder.AppendLine(Keywords.AND);
                return context;
            }
            case AbaScriptParser.OrExprContext orExpr:
            {
                Visit(orExpr.logicalExpr());
                Visit(orExpr.condition());
                _stringBuilder.AppendLine(Keywords.OR);
                return context;
            }
            case AbaScriptParser.ConditionExprContext conditionExpr:
                Visit(conditionExpr.condition());
                return context;
            default:
                throw new InvalidOperationException("Unsupported logical expression");
        }
    }

    public override object? VisitCondition(AbaScriptParser.ConditionContext context)
    {
        if (context.logicalExpr() != null)
        {
            Visit(context.logicalExpr());
            if (context.NOT() != null)
            {
                _stringBuilder.AppendLine(Keywords.NOT);
            }

            return context;
        }

        Visit(context.expr(0));
        Visit(context.expr(1));
        
        var operatorText = context.GetChild(1).GetText();
        switch (operatorText)
        {
            case "==":
                _stringBuilder.AppendLine(Keywords.EQ);
                break;
            case "!=":
                _stringBuilder.AppendLine(Keywords.NE);
                break;
            case "<":
                _stringBuilder.AppendLine(Keywords.LT);
                break;
            case "<=":
                _stringBuilder.AppendLine(Keywords.LE);
                break;
            case ">":
                _stringBuilder.AppendLine(Keywords.GT);
                break;
            case ">=":
                _stringBuilder.AppendLine(Keywords.GE);
                break;
            default:
                throw new InvalidOperationException("Unsupported operation");
        }
        return context;
    }

    public override object VisitOutputStatement(AbaScriptParser.OutputStatementContext context)
    {
        Visit(context.expr());
        _stringBuilder.AppendLine("PRINT");

        return context;
    }
}