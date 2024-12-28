using System.Reflection;
using AbaScript;
using AbaScript.AntlrClasses;
using AbaScript.InterpreterClasses;
using Antlr4.Runtime;

var path = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\..\\..\\..\\..\\example.as";
var input = File.ReadAllText(path);
var lexer = new AbaScriptLexer(new AntlrInputStream(input));
var tokens = new CommonTokenStream(lexer);
var parser = new AbaScriptParser(tokens);

var errorListener = new AbaScriptCustomListener();
lexer.RemoveErrorListeners();
lexer.AddErrorListener(errorListener);
parser.RemoveErrorListeners();
parser.AddErrorListener(errorListener);

var tree = parser.script();

if (errorListener.HasErrors)
{
    Console.WriteLine("Errors detected. Stopping execution.");
    return;
}

// var visitor = new LlvmVisitor();
var visitor = new AbaScriptInterpreter();
visitor.Visit(tree);