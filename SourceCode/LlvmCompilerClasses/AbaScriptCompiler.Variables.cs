﻿using LLVMSharp.Interop;

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
        Visit(context.expr());
        var currentElement = _valueStack.Pop();

        switch (currentElement.TypeOf.Kind)
        {
            case LLVMTypeKind.LLVMIntegerTypeKind:
                Console.WriteLine(GetIntFromRef(currentElement));
                break;
            case LLVMTypeKind.LLVMArrayTypeKind:
                var asString = currentElement.ToString();
                var index = asString.IndexOf(']');
                Console.WriteLine(asString.Substring(index + 4, asString.Length - index - 8));
                break;
        }

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