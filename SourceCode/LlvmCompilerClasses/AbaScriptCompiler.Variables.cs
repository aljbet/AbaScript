using AbaScript.AntlrClasses;
using LLVMSharp.Interop;

namespace AbaScript.LlvmCompilerClasses;

public partial class AbaScriptCompiler
{
    public override object VisitVariableDeclaration(AbaScriptParser.VariableDeclarationContext context)
    {
        // TODO: пофиксить повторное объявление переменных

        var varType = context.type().GetText();
        var varName = context.ID().GetText();
        
        var varTypeLlvm = TypeMatch(varType);

        if (context.NUMBER() != null) // array
        {
            var size = ulong.Parse(context.NUMBER().GetText());
            var sizeLlvm = LLVMValueRef.CreateConstInt(_context.Int32Type, size);
            var malloc = _builder.BuildArrayMalloc(varTypeLlvm, sizeLlvm);
            
            var alloca = _builder.BuildAlloca(varTypeLlvm);
            alloca.Name = varName;
            
            _scopeManager[varName] = new ArrayAllocaInfo(alloca, varTypeLlvm, size);

            _builder.BuildStore(malloc, alloca);
        }
        else { // int
            LLVMValueRef value;
            if (context.expr() != null)
            {
                Visit(context.expr());
                value = _valueStack.Pop();
                if (!CheckType(varType, value.TypeOf.Kind))
                    throw new InvalidOperationException($"Переменная {varName} должна быть типа {varType}.");
            }
            else
            {
                value = LLVMValueRef.CreateConstInt(_intType, 0); // default value
            }
            var alloca = _builder.BuildAlloca(varTypeLlvm);
            alloca.Name = varName;
            _valueStack.Push(_builder.BuildStore(value, alloca));

            _scopeManager[varName] = new AllocaInfo(alloca, varTypeLlvm);
            
            _logger.Log($"Переменная {varName} объявлена со значением: {value} (тип: {varType})");
        }

        return context;
    }

    public override object VisitAssignment(AbaScriptParser.AssignmentContext context)
    {
        var expressions = context.expr();
        var varName = context.ID().GetText();

        if (expressions.Length == 2) // array element
        {
            Visit(expressions[0]);
            var index = _valueStack.Pop();

            Visit(expressions[1]);
            var value = _valueStack.Pop();
            
            if (!_scopeManager.TryGetValue(varName, out var variable) || !(variable is ArrayAllocaInfo))
                throw new InvalidOperationException($"Переменная '{varName}' не объявлена или не является массивом.");
            
            if (value.TypeOf.Kind != variable.Ty.Kind)
                throw new InvalidOperationException($"Переменная {varName} должна быть типа {variable.Ty.Kind}.");

            LLVMValueRef vectorIndexPtr = _builder.BuildGEP2(variable.Ty, variable.Alloca, new LLVMValueRef[] { index });
            _builder.BuildStore(value, vectorIndexPtr);
        }
        else
        {
            Visit(expressions[0]);
            LLVMValueRef value = _valueStack.Pop();

            if (!_scopeManager.TryGetValue(varName, out var variable))
                throw new InvalidOperationException($"Переменная '{varName}' не объявлена.");
            
            if (value.TypeOf.Kind != variable.Ty.Kind)
                throw new InvalidOperationException($"Переменная {varName} должна быть типа {variable.Ty.Kind}.");

            _builder.BuildStore(value, variable.Alloca);
            _logger.Log($"Переменная {varName} обновлена: {value} (тип: {variable.Ty.Kind})");
        }

        return context;
    }

    public override object VisitVariableOrArrayAccess(AbaScriptParser.VariableOrArrayAccessContext context)
    {
        string variableName = context.ID().GetText();
        if (context.expr() != null) // array
        {
            // TODO: обрабатывать границы массива
            Visit(context.expr());
            var index = _valueStack.Pop();

            if (!_scopeManager.TryGetValue(variableName, out var variable) || !(variable is ArrayAllocaInfo))
                throw new InvalidOperationException($"Переменная '{variableName}' не объявлена или не является массивом.");
            
            LLVMValueRef vectorIndexPtr = _builder.BuildGEP2(variable.Ty, variable.Alloca, new LLVMValueRef[] { index });
            _valueStack.Push(_builder.BuildLoad2(variable.Ty, vectorIndexPtr));
        }
        else // int
        {
            if (_scopeManager.TryGetValue(variableName, out var alloca))
            {
                _valueStack.Push(_builder.BuildLoad2(alloca.Ty, alloca.Alloca));
            }
            else
            {
                throw new InvalidOperationException($"Переменная '{variableName}' не объявлена.");
            }
        }

        return context;
    }
/*
    public override object VisitInputStatement(AbaScriptParser.InputStatementContext context)
    {
        var varName = context.ID().GetText();
        Console.Write($"Введите значение для {varName}: ");
        var input = Console.ReadLine();

        if (!_variables.ContainsKey(varName))
            throw new InvalidOperationException($"Переменная '{varName}' не объявлена.");
        var value = TryParseNumber(input);
        switch (value)
        {
            case int valueInt:
                _variables[varName] = GetRefFromInt(valueInt);
                break;
            case string valueStr:
                _variables[varName] = GetRefFromString(valueStr);
                break;
        }

        return context;
    }*/

    public override object VisitOutputStatement(AbaScriptParser.OutputStatementContext context)
    {
        // TODO: пока что подразумевается, что принтится int (для элементов массива тоже работает)
        
        Visit(context.expr());
        var currentElement = _valueStack.Pop();
        
        var printFnTy = LLVMTypeRef.CreateFunction(_context.Int32Type, new[] {_intType});
        var call = _builder.BuildCall2(printFnTy, GetOrCreatePrintFunc(), new[] {currentElement});
        
        _valueStack.Push(call);
        // // switch (currentElement.TypeOf.Kind)
        // // {
        // //     case LLVMTypeKind.LLVMIntegerTypeKind:
        // //         Console.WriteLine(GetIntFromRef(currentElement));
        // //         break;
        // //     case LLVMTypeKind.LLVMArrayTypeKind:
        // //         Console.WriteLine(GetStringFromRef(currentElement));
        // //         break;
        // // }
        // //
        return context;
    }

    public override object VisitNumber(AbaScriptParser.NumberContext context)
    {
        if (int.TryParse(context.GetText(), out var number))
        {
            _valueStack.Push(GetRefFromInt(number));
            return context;
        }

        throw new InvalidOperationException($"Невозможно преобразовать в число: {context.GetText()}");
    }

    public override object VisitString(AbaScriptParser.StringContext context)
    {
        var str = context.GetText().Trim('"');
        _valueStack.Push(GetRefFromString(str));

        return context;
    }

    private LLVMValueRef GetOrCreatePrintFunc()
    {
        var printName = "$print";
        var printFnTy = LLVMTypeRef.CreateFunction(_context.Int32Type, new[] {_intType});
        var printFn = _module.GetNamedFunction(printName);
        if (printFn != null)
        {
            return printFn;
        }
        
        printFn = _module.AddFunction(printName, printFnTy);
        _builder.PositionAtEnd(printFn.AppendBasicBlock("entry"));
        
        LLVMTypeRef argumentTy = LLVMTypeRef.Int32;
        LLVMValueRef param = printFn.Params[0];
        param.Name = "x";
        var alloca = _builder.BuildAlloca(argumentTy);
        _builder.BuildStore(param, alloca);
        
        var funcName = "puts"; // можно поменять на printf
        var putsFnTy = LLVMTypeRef.CreateFunction(_context.Int32Type, new[] {LLVMTypeRef.CreatePointer(_context.Int8Type, 0)});
        var putsFn = _module.GetNamedFunction(funcName);
        if (putsFn == null)
        {
            putsFn = _module.AddFunction(funcName, putsFnTy);
        }

        var ptrType = LLVMTypeRef.CreatePointer(_context.Int8Type, 0);
        
        var buffer = _builder.BuildAlloca(ptrType, "print.buffer");
        var bufferSize = LLVMValueRef.CreateConstInt(_context.Int32Type, 1024 * 4); // TODO: подумать каким должен быть N
        _builder.BuildStore(_builder.BuildArrayMalloc(_context.Int8Type, bufferSize), buffer);
        var originalBuffer = _builder.BuildLoad2(ptrType, buffer);
        
        // save value to buff
        var currentElement = _builder.BuildLoad2(argumentTy, alloca);
        var args = new[]{originalBuffer, _builder.BuildGlobalStringPtr("%d"), currentElement};
        var sprintfFn = _module.GetNamedFunction("sprintf");
        var sprintfFnTy = LLVMTypeRef.CreateFunction(_context.Int32Type, new[] {
            LLVMTypeRef.CreatePointer(_context.Int8Type, 0),
            LLVMTypeRef.CreatePointer(_context.Int8Type, 0)
        }, true);
        if (sprintfFn == null)
        {
            sprintfFn = _module.AddFunction("sprintf", sprintfFnTy);
        }
        
        _builder.BuildCall2(sprintfFnTy, sprintfFn, args);
        
        // print
        _builder.BuildCall2(putsFnTy, putsFn, new []{ originalBuffer });
        
        _builder.BuildFree(originalBuffer);
        
        _builder.BuildRet(LLVMValueRef.CreateConstInt(_context.Int32Type, 0));

        // Validate the generated code, checking for consistency.
        printFn.VerifyFunction(LLVMVerifierFailureAction.LLVMAbortProcessAction);
        
        return printFn;
    }
}