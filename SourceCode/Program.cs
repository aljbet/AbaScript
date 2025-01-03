using System.Reflection;
using AbaScript;
using AbaScript.AntlrClasses;
using AbaScript.LlvmCompilerClasses;
using Antlr4.Runtime;
using LLVMSharp.Interop;

var path = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) + "\\..\\..\\..\\..\\example.as";
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

var context = LLVMContextRef.Create();
var module = context.CreateModuleWithName("AbaScript");
var builder = context.CreateBuilder();
var visitor = new AbaScriptCompiler(context, module, builder);
// var visitor = new AbaScriptCustomVisitor();
visitor.Visit(tree);
Console.WriteLine($"LLVM IR\n=========\n{module}");

// Initialize LLVM
LLVM.InitializeAllTargetInfos();
LLVM.InitializeAllTargets();
LLVM.InitializeAllTargetMCs();
LLVM.InitializeAllAsmParsers();
LLVM.InitializeAllAsmPrinters();

var triple = LLVMTargetRef.DefaultTriple;

Console.WriteLine($"Targeting {triple}");

// Create the target machine
var target = LLVMTargetRef.GetTargetFromTriple(triple);
var targetMachine = target.CreateTargetMachine(
    triple, "generic", "",
    LLVMCodeGenOptLevel.LLVMCodeGenLevelNone,
    LLVMRelocMode.LLVMRelocDefault,
    LLVMCodeModel.LLVMCodeModelDefault);

var outFile = "out.o";

targetMachine.EmitToFile(module, outFile, LLVMCodeGenFileType.LLVMObjectFile);
Console.WriteLine($"Compiled to {outFile}");

// Can also directly set the module triple
// module.Target = triple;
// module.WriteBitcodeToFile("out2.o");

// Run with the Just-In Time engine
if (args.Contains("jit"))
{
    var engine = module.CreateExecutionEngine();
    var main = module.GetNamedFunction("main");
    engine.RunFunctionAsMain(main, 0, Array.Empty<string>(), Array.Empty<string>());
}

using var linkProcess = System.Diagnostics.Process.Start("gcc", new[] { outFile }); // сборка exe

await linkProcess.WaitForExitAsync();
Console.WriteLine($"Linked with standard library");
Console.WriteLine("Тут начинается то, что выведет исполнение example.as");

using var runProcess = System.Diagnostics.Process.Start("a.exe"); // запуск

await runProcess.WaitForExitAsync();
Console.WriteLine($"Тут заканчивается то, что вывело исполнение example.as");