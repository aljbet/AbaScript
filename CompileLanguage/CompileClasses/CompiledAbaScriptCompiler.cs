using System.Text;
using AbaScript.AntlrClasses;

namespace CompileLanguage.CompileClasses;

public class CompiledAbaScriptCompiler : AbaScriptBaseVisitor<object>
{
    private StringBuilder _compiledCode = new StringBuilder();
    private int _labelCounter;

    public string Compile(string abaScriptCode)
    {
        _compiledCode.Clear();
        
        Visit(program);

        return _compiledCode.ToString();
    }

    private string GenerateUniqueLabel()
    {
        return $"label_{_labelCounter++}";
    }

    public override object VisitVariableDeclaration(AbaScriptParser.VariableDeclarationContext context)
    {
        // Variable declarations are handled during expression compilation in CompiledAbaScript.
        return null;
    }

    public override object VisitAssignment(AbaScriptParser.AssignmentContext context)
    {
        Visit(context.expr());
        _compiledCode.AppendLine($"STORE {context.ID()}");
        return null;
    }

    public override object VisitAddSub(AbaScriptParser.AddSubContext context)
    {
        Visit(context.expr(0));
        Visit(context.expr(1));
        _compiledCode.AppendLine(context.op.Text == "+" ? "ADD" : "SUB");
        return null;
    }

    public override object VisitMulDivMod(AbaScriptParser.MulDivModContext context)
    {
        Visit(context.expr(0));
        Visit(context.expr(1));

        _compiledCode.AppendLine(context.op.Text switch
        {
            "*" => "MUL",
            "/" => "DIV",
            "%" => "MOD",
            _ => throw new NotSupportedException($"Unknown operator: {context.op.Text}")
        });
        return null;
    }

    public override object VisitNumberExpr(AbaScriptParser.NumberExprContext context)
    {
        _compiledCode.AppendLine($"PUSH {context.NUMBER()}");
        return null;
    }

    public override object VisitStringExpr(AbaScriptParser.StringExprContext context)
    {
        _compiledCode.AppendLine($"PUSH {context.STRING()}");
        return null;
    }

    public override object VisitIdExpr(AbaScriptParser.IdExprContext context)
    {
        _compiledCode.AppendLine($"LOAD {context.ID()}");
        return null;
    }

    public override object VisitIfStatement(AbaScriptParser.IfStatementContext context)
    {
        var endLabel = GenerateUniqueLabel();
        Visit(context.expr());
        _compiledCode.AppendLine($"JMP_IF {endLabel}");
        Visit(context.block());
        _compiledCode.AppendLine($"{endLabel}:");
        return null;
    }

    public override object VisitWhileStatement(AbaScriptParser.WhileStatementContext context)
    {
        var loopStart = GenerateUniqueLabel();
        var loopEnd = GenerateUniqueLabel();

        _compiledCode.AppendLine($"{loopStart}:");
        Visit(context.expr());
        _compiledCode.AppendLine($"JMP_IF {loopEnd}");
        Visit(context.block());
        _compiledCode.AppendLine($"JMP {loopStart}");
        _compiledCode.AppendLine($"{loopEnd}:");

        return null;
    }


    public override object VisitPrintStatement(AbaScriptParser.PrintStatementContext context)
    {
        Visit(context.expr());
        _compiledCode.AppendLine("PRINT");
        return null;
    }
}