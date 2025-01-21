using Antlr4.Runtime.Tree;

namespace CompileLanguage.Services;

public interface IParseTreeCreator
{
    IParseTree CreateTree(string text);
}