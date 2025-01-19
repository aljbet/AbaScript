using AbaScript.AntlrClasses;
using LLVMSharp.Interop;

namespace AbaScript.LlvmCompilerClasses;

public partial class AbaScriptCompiler
{
    public override object VisitFunctionDef(AbaScriptParser.FunctionDefContext context)
    {
        var argumentCount = context.typedParam().Length;
        var arguments = new LLVMTypeRef[argumentCount];
        var funcName = context.ID().GetText();

        var calleeF = _module.GetNamedFunction(funcName);

        if (calleeF != null)
        {
            throw new InvalidOperationException($"Функция '{funcName}' уже определена.");
        }
        
        var parameters = context.typedParam().Select(p => (p.type().GetText(), p.ID().GetText(), p.GetText())).ToList();
        for (var i = 0; i < argumentCount; ++i)
        {
            arguments[i] = TypeMatch(parameters[i].Item1);
        }

        var returnType = TypeMatch(context.returnType().GetText());

        var funcType = LLVMTypeRef.CreateFunction(returnType, arguments);
        _funcTypes[funcName] = funcType;
        var function = _module.AddFunction(funcName, funcType);

        _scopeManager.EnterScope();

        // Create a new basic block to start insertion into.
        var block = function.AppendBasicBlock("entry");
        _builder.PositionAtEnd(block);

        for (int i = 0; i < argumentCount; ++i)
        {
            string argumentName = parameters[i].Item2;
            LLVMTypeRef argumentTy = arguments[i];

            LLVMValueRef param = function.Params[i];
            param.Name = argumentName;

            var alloca = _builder.BuildAlloca(argumentTy);
            _builder.BuildStore(param, alloca);
            if (parameters[i].Item3.Contains('['))
            {
                _scopeManager[argumentName] = new ArrayAllocaInfo(alloca, argumentTy, 0);
            }
            else
            {
                _scopeManager[argumentName] = new AllocaInfo(alloca, argumentTy);
            }
        }

        try
        {
            Visit(context.block());
            _valueStack.Pop();
        }
        catch (Exception)
        {
            _scopeManager.ExitScope();
            function.DeleteFunction();
            _funcTypes.Remove(funcName);
            throw;
        }

        // Finish off the function.
        _builder.BuildRet(LLVMValueRef.CreateConstInt(_intType, 0));
        ClearAfterReturn(block);
        foreach (var funcBlock in function.BasicBlocks)
        {
            ClearAfterReturn(funcBlock);
        }

        // Validate the generated code, checking for consistency.
        function.VerifyFunction(LLVMVerifierFailureAction.LLVMAbortProcessAction);
        //function.VerifyFunction(LLVMVerifierFailureAction.LLVMPrintMessageAction);

        _valueStack.Push(function);

        _logger.Log(
            $"Функция {funcName} определена с параметрами: {string.Join(", ", parameters.Select(p => $"{p.Item1} {p.Item2}"))}");

        _scopeManager.ExitScope();

        return context;
    }

    public override object VisitFuncCall(AbaScriptParser.FuncCallContext context)
    {
        var funcName = context.ID().GetText();
        var calleeF = _module.GetNamedFunction(funcName);

        if (calleeF == null)
        {
            throw new InvalidOperationException($"Функция '{funcName}' не определена.");
        }

        var arguments = new List<LLVMValueRef>();
        foreach (var expr in context.expr())
        {
            Visit(expr);
            arguments.Add(_valueStack.Pop());
        }

        if (calleeF.ParamsCount != arguments.Count)
        {
            throw new InvalidOperationException($"Количество аргументов не совпадает для функции '{funcName}'.");
        }

        for (var i = 0; i < arguments.Count; i++)
        {
            var expectedType = calleeF.Params[i].TypeOf.Kind;
            var actualType = arguments[i].TypeOf.Kind;
            if (expectedType != actualType)
                throw new InvalidOperationException(
                    $"Аргумент {i} функции {funcName} должен быть типа {expectedType} (получен {actualType}).");
        }

        var funcType = _funcTypes[funcName];
        _valueStack.Push(_builder.BuildCall2(funcType, calleeF, arguments.ToArray()));
        return context;
    }

    public override object VisitReturnStatement(AbaScriptParser.ReturnStatementContext context)
    {
        Visit(context.expr());
        _valueStack.Push(_builder.BuildRet(_valueStack.Pop()));

        return context;
    }
}