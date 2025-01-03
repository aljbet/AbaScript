using LLVMSharp.Interop;

namespace AbaScript.LlvmCompilerClasses;

public partial class AbaScriptCompiler
{
    public override object VisitVariableDeclaration(AbaScriptParser.VariableDeclarationContext context)
    {
        var varType = context.type().GetText();
        var varName = context.ID().GetText();
        LLVMValueRef value = null;

        if (context.expr() != null)
        {
            Visit(context.expr());
            value = _valueStack.Pop();
            if (!CheckType(varType, value.TypeOf.Kind))
                throw new InvalidOperationException($"Переменная {varName} должна быть типа {varType}.");
        }

        _variables[varName] = value;
        _logger.Log($"Переменная {varName} объявлена со значением: {value} (тип: {varType})");

        return context;
    }

    public override object VisitAssignment(AbaScriptParser.AssignmentContext context)
    {
        var expressions = context.expr();
        var varName = context.ID().GetText();
        Visit(expressions[0]);
        LLVMValueRef value = _valueStack.Pop();

        if (!_variables.TryGetValue(varName, out var variable))
            throw new InvalidOperationException($"Переменная '{varName}' не объявлена.");

        if (value.TypeOf.Kind != variable.TypeOf.Kind)
            throw new InvalidOperationException($"Переменная {varName} должна быть типа {variable.TypeOf.Kind}.");
        _variables[varName] = value;
        _logger.Log($"Переменная {varName} обновлена: {value} (тип: {variable.TypeOf.Kind})");

        return context;
    }


    public override object VisitVariableOrArrayAccess(AbaScriptParser.VariableOrArrayAccessContext context)
    {
        string variableName = context.ID().GetText();
        if (_variables.TryGetValue(variableName, out var value))
        {
            _valueStack.Push(value);
        }
        else
        {
            throw new InvalidOperationException($"Переменная '{variableName}' не объявлена.");
        }

        return context;
    }

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
    }

    public override object VisitOutputStatement(AbaScriptParser.OutputStatementContext context)
    {
        // TODO: пока что подразумевается, что принтится int
        var funcName = "puts"; // можно поменять на printf
        var putsRetTy = _context.Int32Type;
        var putsParamTys = new LLVMTypeRef[] {
            LLVMTypeRef.CreatePointer(_context.Int8Type, 0)
        };
        var putsFnTy = LLVMTypeRef.CreateFunction(putsRetTy, putsParamTys);
        var putsFn = _module.GetNamedFunction(funcName);
        if (putsFn == null)
        {
            putsFn = _module.AddFunction(funcName, putsFnTy);
        }
        
        Visit(context.expr());
        var currentElement = _valueStack.Pop();

        var ptrType = LLVMTypeRef.CreatePointer(_context.Int8Type, 0);
        
        var buffer = _builder.BuildAlloca(ptrType, "print.buffer");
        var bufferSize = LLVMValueRef.CreateConstInt(_context.Int32Type, 1024 * 4, false); // TODO: подумать каким должен быть N
        _builder.BuildStore(_builder.BuildArrayMalloc(_context.Int8Type, bufferSize, ""), buffer);
        var originalBuffer = _builder.BuildLoad2(ptrType, buffer, "");

        // save value to buff
        var args = new LLVMValueRef[]{originalBuffer, _builder.BuildGlobalStringPtr("%d", ""), currentElement};
        var sprintfFn = _module.GetNamedFunction("sprintf");
        var sprintfFnTy = LLVMTypeRef.CreateFunction(_context.Int32Type, new LLVMTypeRef[] {
            LLVMTypeRef.CreatePointer(_context.Int8Type, 0),
            LLVMTypeRef.CreatePointer(_context.Int8Type, 0)
        }, true);
        if (sprintfFn == null)
        {
            sprintfFn = _module.AddFunction("sprintf", sprintfFnTy);
        }
        
        var count = _builder.BuildCall2(sprintfFnTy, sprintfFn, args, "");

        // print
        _builder.BuildCall2(putsFnTy, putsFn, new LLVMValueRef[] { originalBuffer }, "");

        _builder.BuildFree(originalBuffer);

        //
        // switch (currentElement.TypeOf.Kind)
        // {
        //     case LLVMTypeKind.LLVMIntegerTypeKind:
        //         Console.WriteLine(GetIntFromRef(currentElement));
        //         break;
        //     case LLVMTypeKind.LLVMArrayTypeKind:
        //         Console.WriteLine(GetStringFromRef(currentElement));
        //         break;
        // }
        //
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
}