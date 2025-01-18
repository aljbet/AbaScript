using AbaScript.LlvmCompilerClasses;
using AbaScript.Services.Interfaces;
using LLVMSharp.Interop;

namespace AbaScript.Services;

public class LlvmService : RunService
{
    public override object? RunCode(string input)
    {
        var tree = SetUpParseTree(input);

        if (ErrorListener.HasErrors)
        {
            Console.WriteLine("Errors detected. Stopping execution.");
            return null;
        }

        var context = LLVMContextRef.Create();
        var module = context.CreateModuleWithName("AbaScript");
        var builder = context.CreateBuilder();
        var visitor = new AbaScriptCompiler(context, module, builder);
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
        
        // Run with the Just-In Time engine
        // if (args.Contains("jit"))
        // {
            // var engine = module.CreateExecutionEngine();
            // var main = module.GetNamedFunction("main");
            // engine.RunFunctionAsMain(main, 0, Array.Empty<string>(), Array.Empty<string>());
        // }

        using var linkProcess = System.Diagnostics.Process.Start("gcc", new[] { outFile }); // сборка exe

        linkProcess.WaitForExit();
        Console.WriteLine("Linked with standard library");
        Console.WriteLine("Тут начинается то, что выведет исполнение файла");

        using var runProcess = System.Diagnostics.Process.Start("a.exe"); // запуск

        runProcess.WaitForExit();
        Console.WriteLine("Тут заканчивается то, что вывело исполнение файла");
        return 0;
    }
}