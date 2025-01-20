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

    public override object VisitOutputStatement(AbaScriptParser.OutputStatementContext context)
    {
        Visit(context.expr());
        _stringBuilder.AppendLine("PRINT");

        return context;
    }
}