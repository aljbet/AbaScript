using System.Collections;
using Antlr4.Runtime.Tree;

namespace AbaScript.AntlrClasses;

public class AbaScriptMainChecker : AbaScriptBaseVisitor<object>
{
    private readonly List<string> _globalVariables = new();
    private bool _hasMainFunction;

    public override object VisitFunctionDef(AbaScriptParser.FunctionDefContext context)
    {
        if (context.ID().GetText() == "main" && context.returnType().GetText() == "int") _hasMainFunction = true;
        return base.VisitFunctionDef(context);
    }

    public override object VisitVariableDeclaration(AbaScriptParser.VariableDeclarationContext context)
    {
        IParseTree parent = context.Parent;
        while (parent != null && parent is not AbaScriptParser.FunctionDefContext) parent = parent.Parent;
        if (parent == null) _globalVariables.Add(context.ID().GetText());
        return base.VisitVariableDeclaration(context);
    }

    public bool HasMainFunction()
    {
        return _hasMainFunction;
    }

    public IList<string> GetGlobalVariables()
    {
        return _globalVariables;
    }
}