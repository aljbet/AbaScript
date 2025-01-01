﻿using System.Reflection;
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