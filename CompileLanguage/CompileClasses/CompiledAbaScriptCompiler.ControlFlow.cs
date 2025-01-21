using AbaScript.AntlrClasses;

namespace CompileLanguage.CompileClasses;

public partial class CompiledAbaScriptCompiler
{
    public override object VisitIfStatement(AbaScriptParser.IfStatementContext context)
    {
        var ifBlockLabel = "if_label_" + _ifBlocksCount;
        var elseBlockLabel = "else_label_" + _ifBlocksCount;
        var ifEndBlockLabel = "if_end_label_" + _ifBlocksCount;
        _ifBlocksCount++;

        Visit(context.logicalExpr(0));
        _stringBuilder.AppendLine($"{Keywords.IF_THEN_ELSE} {ifBlockLabel} {elseBlockLabel}");

        _stringBuilder.AppendLine(ifBlockLabel + ":");
        _stringBuilder.AppendLine(Keywords.ENTER_SCOPE);
        Visit(context.block(0));
        _stringBuilder.AppendLine(Keywords.EXIT_SCOPE);
        _stringBuilder.AppendLine($"{Keywords.JMP} {ifEndBlockLabel}");

        _stringBuilder.AppendLine(elseBlockLabel + ":");
        _stringBuilder.AppendLine(Keywords.ENTER_SCOPE);
        Visit(context.block(1));
        _stringBuilder.AppendLine(Keywords.EXIT_SCOPE);
        _stringBuilder.AppendLine($"{Keywords.JMP} {ifEndBlockLabel}");

        _stringBuilder.AppendLine(ifEndBlockLabel + ":");

        return context;
    }

    public override object VisitForStatement(AbaScriptParser.ForStatementContext context)
    {
        var forBlockLabel = "for_label_" + _forBlocksCount;
        var forLogicLabel = "for_logic_label" + _forBlocksCount;
        var forEndBlockLabel = "for_end_label_" + _forBlocksCount;
        _forBlocksCount++;

        _stringBuilder.AppendLine(Keywords.ENTER_SCOPE);
        if (context.variableDeclaration() != null)
        {
            Visit(context.variableDeclaration());
        }
        else
        {
            Visit(context.assignment(0));
        }

        _stringBuilder.AppendLine(forLogicLabel + ":");
        Visit(context.logicalExpr());
        _stringBuilder.AppendLine($"{Keywords.IF_THEN_ELSE} {forBlockLabel} {forEndBlockLabel}");

        _stringBuilder.AppendLine(forBlockLabel + ":");
        Visit(context.block());

        if (context.variableDeclaration() != null)
        {
            Visit(context.assignment(0));
        }
        else
        {
            Visit(context.assignment(1));
        }

        _stringBuilder.AppendLine($"{Keywords.JMP} {forLogicLabel}");

        _stringBuilder.AppendLine(forEndBlockLabel + ":");
        _stringBuilder.AppendLine(Keywords.EXIT_SCOPE);

        return context;
    }
}