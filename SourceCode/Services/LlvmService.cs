using System.ComponentModel;
using System.Runtime.InteropServices;
using AbaScript.LlvmCompilerClasses;
using AbaScript.Services.Interfaces;
using LLVMSharp.Interop;

namespace AbaScript.Services;

public class LlvmService : RunService
{
    public bool IsOptimizationsNeeded { get; set; }

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
        
        // Initialize LLVM
        LLVM.InitializeAllTargetInfos();
        LLVM.InitializeAllTargets();
        LLVM.InitializeAllTargetMCs();
        LLVM.InitializeAllAsmParsers();
        LLVM.InitializeAllAsmPrinters();
        
        // Passes
        if (IsOptimizationsNeeded)
        {
            var passManager = LLVMPassManagerRef.Create();
            passManager.AddCFGSimplificationPass(); // block merge and dead code elimination
            passManager.AddTailCallEliminationPass();
            passManager.AddPromoteMemoryToRegisterPass();
            passManager.AddLoopUnrollPass();

            passManager.Run(module);
        }

        Console.WriteLine($"LLVM IR\n=========\n{module}");

        var triple = LLVMTargetRef.DefaultTriple;
        Console.WriteLine($"Targeting {triple}");

        // Create the target machine
        var target = LLVMTargetRef.GetTargetFromTriple(triple);
        var targetMachine = target.CreateTargetMachine(
            triple, "generic", "",
            LLVMCodeGenOptLevel.LLVMCodeGenLevelNone,
            LLVMRelocMode.LLVMRelocDefault,
            LLVMCodeModel.LLVMCodeModelDefault);

        // Run with the Just-In Time engine
        Console.WriteLine("Тут начинается то, что выведет исполнение файла");
        LLVM.LinkInMCJIT();
        var engine = module.CreateMCJITCompiler();
        var main = module.GetNamedFunction("main");
        engine.RunFunctionAsMain(main, 0, Array.Empty<string>(), Array.Empty<string>());
        Console.WriteLine("Тут заканчивается то, что вывело исполнение файла");

        return 0;
    }
}