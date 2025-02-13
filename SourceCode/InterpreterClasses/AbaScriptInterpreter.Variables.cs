﻿using AbaScript.AntlrClasses;
using AbaScript.AntlrClasses.Models;

namespace AbaScript.InterpreterClasses;

public partial class AbaScriptInterpreter
{
    public override object VisitVariableDeclaration(AbaScriptParser.VariableDeclarationContext context)
    {
        var varType = context.type().GetText()!;
        var varName = context.ID().GetText();
        object value = null;

        if (context.NUMBER() != null)
        {
            var size = long.Parse(context.NUMBER().GetText());
            value = new object[size];
        }
        else if (context.expr() != null)
        {
            value = Visit(context.expr());
        }

        VariableType variableType;
        if (!Enum.TryParse<VariableType>(varType, true, out variableType))
        {
            // Значит, это не Int/String/Array и т.п. Скорее всего, класс.
            // Дополнительно можно проверить, определён ли такой класс в classDefinitions
            if (!_classDefinitions.ContainsKey(varType))
                throw new InvalidOperationException($"Класс '{varType}' не определён.");

            variableType = VariableType.Class;
        }

        _variables[varName] = new Variable(variableType, value, variableType == VariableType.Class ? varType : null);
        _logger.Log($"Переменная {varName} объявлена со значением: {value} (тип: {varType})");

        return null;
    }

    public override object VisitVariableOrArrayAccess(AbaScriptParser.VariableOrArrayAccessContext context)
    {
        var variableName = context.ID().GetText();
        if (context.expr() != null)
        {
            var index = (long)Visit(context.expr());
            if (!_variables.TryGetValue(variableName, out var variable) || !(variable.Value is object[] array))
                throw new InvalidOperationException(
                    $"Переменная '{variableName}' не объявлена или не является массивом.");
            return array[index];
        }
        else
        {
            if (!_variables.TryGetValue(variableName, out var variable))
                throw new InvalidOperationException($"Переменная '{variableName}' не объявлена.");
            return variable.Value;
        }
    }

    public override object VisitAssignment(AbaScriptParser.AssignmentContext context)
    {
        var expressions = context.expr();

        if (context.fieldAccess() != null)
        {
            // Присвоение полю класса
            var instanceName = context.fieldAccess().ID(0).GetText();
            var fieldName = context.fieldAccess().ID(1).GetText();

            if (!_classInstances.TryGetValue(instanceName, out var instance))
                throw new InvalidOperationException($"Экземпляр '{instanceName}' не существует.");

            // Определяем имя класса
            var className = instance.GetType().Name;
            if (!_classDefinitions[className].Fields.ContainsKey(fieldName))
                throw new InvalidOperationException($"Поле '{fieldName}' не определено в классе '{className}'.");

            var value = Visit(expressions[0]);
            instance.Fields[fieldName] = value;
            _logger.Log($"Поле {fieldName} экземпляра {instanceName} обновлено: {value}");
        }
        else
        {
            var varName = context.ID().GetText();

            if (expressions.Length == 2)
            {
                // array[index] = value;
                var index = (long)Visit(expressions[0]);
                if (!_variables.TryGetValue(varName, out var variable) || !(variable.Value is object[] array))
                    throw new InvalidOperationException($"Переменная '{varName}' не является массивом.");

                array[index] = Visit(expressions[1]);
            }
            else
            {
                var value = Visit(expressions[0]);
                if (!_variables.TryGetValue(varName, out var variable))
                    throw new InvalidOperationException($"Переменная '{varName}' не объявлена.");

                if (!CheckType(variable.Type.ToString(), value))
                    throw new InvalidOperationException(
                        $"Переменная {varName} должна быть типа {variable.Type}.");

                variable.Value = value;
                _logger.Log($"Переменная {varName} обновлена: {value} (тип: {variable.Type})");
            }
        }

        return null;
    }

    public override object VisitInputStatement(AbaScriptParser.InputStatementContext context)
    {
        var varName = context.ID().GetText();
        Console.Write($"Введите значение для {varName}: ");
        var input = Console.ReadLine();

        if (context.expr() != null)
        {
            // Ввод в массив
            var index = (int)Visit(context.expr());
            if (!_variables.TryGetValue(varName, out var variable) || !(variable.Value is object[] array))
                throw new InvalidOperationException($"Переменная '{varName}' не объявлена или не является массивом.");
            array[index] = TryParseNumber(input);
        }
        else
        {
            // Ввод в обычную переменную
            if (!_variables.TryGetValue(varName, out var variable))
                throw new InvalidOperationException($"Переменная '{varName}' не объявлена.");
            variable.Value = TryParseNumber(input);
        }

        return null;
    }

    public override object VisitOutputStatement(AbaScriptParser.OutputStatementContext context)
    {
        var value = Visit(context.expr());
        Console.WriteLine(value);
        return null;
    }
}