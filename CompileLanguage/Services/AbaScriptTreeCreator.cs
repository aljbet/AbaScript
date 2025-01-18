using AbaScript.AntlrClasses;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace CompileLanguage.Services;

public class AbaScriptTreeCreator : IParseTreeCreator
{
    public IParseTree CreateTree(string text)
    {
        var lexer = new AbaScriptLexer(new AntlrInputStream(text));
        var tokens = new CommonTokenStream(lexer);
        var parser = new AbaScriptParser(tokens);

        var errorListener = new AbaScriptLexerAndParserErrorListener();
        lexer.RemoveErrorListeners();
        lexer.AddErrorListener(errorListener);
        parser.RemoveErrorListeners();
        parser.AddErrorListener(errorListener);

        var tree = parser.script();
        return tree;
    }
}