using CompileLanguage.CompileClasses;
using CompileLanguage.InterpreterClasses;

namespace CompileLanguage.Services;

public class CompilerService : IExecutable
{
    private CompiledAbaScriptCompiler _compiler = new CompiledAbaScriptCompiler();
    private CompiledAbaScriptInterpreter _interpreter = new CompiledAbaScriptInterpreter(new Dictionary<string, ClassInfo>());
    private readonly IParseTreeCreator _treeCreator = new AbaScriptTreeCreator();
    
    public void Execute(string input)
    {
        var tree = _treeCreator.CreateTree(input);

        _compiler = new CompiledAbaScriptCompiler();
        _interpreter = new CompiledAbaScriptInterpreter(_compiler.GetClassesContext());
        var compiledCode = _compiler.Visit(tree);
        if (compiledCode is not string code)
        {
            throw new ArgumentException("Compiled code has to be string");
        }
        Console.WriteLine(code);
        _interpreter.Interpret(code);
    }
}