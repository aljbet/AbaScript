using AbaScript.AntlrClasses;

namespace CompileLanguage.CompileClasses;

public partial class CompiledAbaScriptCompiler
{
    public override object? VisitFunctionDef(AbaScriptParser.FunctionDefContext context)
    {
        _stringBuilder.Append(context.ID() + ":");
        // надо добавить в словарь functionLabels но я хз какой тут i
        for (int i = context.typedParam().Length - 1; i >= 0; i--)
        {
            _stringBuilder.Append($"{Keywords.STORE} { context.typedParam(i).ID()}");
        }

        VisitBlock(context.block());

        /*
         * в конец надо добавить jmp на место, откуда его вызвали, но как его запоминать?
         * можем прыгать только по лейблам, получается каждый вызов функции отдельный лейбл?...
         * сделала стек returnPoints, должен как раз хранить числа, соответствующие нужным лейблам.
         */
        
        return context;
    }

    public override object? VisitBlock(AbaScriptParser.BlockContext context)
    {
        foreach (var statement in context.statement())
        {
            Visit(statement);
        }
        
        return context;
    }
}