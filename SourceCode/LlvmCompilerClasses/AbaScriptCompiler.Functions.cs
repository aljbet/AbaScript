using LLVMSharp.Interop;

namespace AbaScript.LlvmCompilerClasses;

public partial class AbaScriptCompiler
{
    public override object VisitFunctionDef(AbaScriptParser.FunctionDefContext context)
    {
        // TODO: не работает сохранение контекста переменных
        var argumentCount = context.typedParam().Length;
        var arguments = new LLVMTypeRef[argumentCount];
        var funcName = context.ID().GetText();
        
        var calleeF = _module.GetNamedFunction(funcName);

        if (calleeF != null)
        {
            throw new InvalidOperationException($"Функция '{funcName}' уже определена.");
        }
        
        var parameters = context.typedParam().Select(p => (p.type().GetText(), p.ID().GetText())).ToList();
        for (int i = 0; i < argumentCount; ++i)
        {
            switch (parameters[i].Item1)
            {
                // TODO: вынести match типов в отдельную функцию
                case "int":
                    // TODO: вынести в константу
                    arguments[i] = _context.GetIntType(32);
                    break;
                default:
                    throw new InvalidOperationException($"Неизвестный тип {parameters[i].Item1}");
                    break;
            }
        }

        LLVMTypeRef returnType;
        switch (context.returnType().GetText())
        {
            case "int":
                returnType = _context.GetIntType(32);
                break;
            default:
                throw new InvalidOperationException($"Неизвестный тип {context.returnType().GetText()}");
        }

        var funcType = LLVMTypeRef.CreateFunction(returnType, arguments);
        _funcTypes[funcName] = funcType; // TODO: придумать, как сделать это средствами llvm
        var function = _module.AddFunction(funcName, funcType);
        
        _variables.Clear();
        
        for (int i = 0; i < argumentCount; ++i)
        {
            string argumentName = parameters[i].Item2;

            LLVMValueRef param = function.Params[i];
            param.Name = argumentName;

            _variables[argumentName] = param;
        }
        
        // Create a new basic block to start insertion into.
        _builder.PositionAtEnd(_context.AppendBasicBlock(function, funcName));
        
        try
        {
            Visit(context.block());
        }
        catch (Exception)
        {
            function.DeleteFunction();
            _funcTypes.Remove(funcName);
            throw;
        }
        
        // Finish off the function.
        _builder.BuildRet(_valueStack.Pop());

        // Validate the generated code, checking for consistency.
        function.VerifyFunction(LLVMVerifierFailureAction.LLVMPrintMessageAction); // TODO: наверное, принтить ошибки не очень хорошая идея

        _valueStack.Push(function);

        _logger.Log(
            $"Функция {funcName} определена с параметрами: {string.Join(", ", parameters.Select(p => $"{p.Item1} {p.Item2}"))}");
        
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
                    $"Аргумент {i} функции {funcName} должен быть типа {expectedType}.");
        }
        
        // TODO: сохранение и восстановление переменных (или это делает llvm?)
        var funcType = _funcTypes[funcName];
        _valueStack.Push(_builder.BuildCall2(funcType, calleeF, arguments.ToArray()));
        return context;
    }
    
    public override object VisitReturnStatement(AbaScriptParser.ReturnStatementContext context)
    {
        Visit(context.expr());

        return context;
    }
}