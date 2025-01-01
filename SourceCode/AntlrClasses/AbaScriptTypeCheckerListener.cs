using System;
using System.Collections.Generic;
using AbaScript.AntlrClasses;
using Antlr4.Runtime.Misc;

public class TypeCheckerListener : AbaScriptBaseListener
{
    // Таблица переменных (имя_переменной -> тип)
    private readonly Dictionary<string, string> symbolTable = new Dictionary<string, string>();

    // Таблица классов: имя_класса -> (имя_поля -> тип_поля)
    private readonly Dictionary<string, Dictionary<string, string>> classTable 
        = new Dictionary<string, Dictionary<string, string>>();

    // Стек для временного хранения типов при обходе выражений
    private readonly Stack<string> typeStack = new Stack<string>();

    // ----------------------
    //  Объявление переменных
    // ----------------------
    public override void EnterVariableDeclaration([NotNull] AbaScriptParser.VariableDeclarationContext context)
    {
        var declaredType = context.type().GetText();
        var varName = context.ID().GetText();
        symbolTable[varName] = declaredType;
    }

    public override void ExitVariableDeclaration([NotNull] AbaScriptParser.VariableDeclarationContext context)
    {
        // Если есть инициализация
        if (context.expr() != null)
        {
            var initExprType = typeStack.Pop();
            var varName = context.ID().GetText();
            var declaredType = symbolTable[varName];

            if (!TypeHelper.AreCompatible(declaredType, initExprType))
            {
                Console.Error.WriteLine($"Ошибка: несоответствие типов при объявлении переменной {varName} " +
                                        $"(ожидался {declaredType}, а получен {initExprType})");
            }
        }
    }

    // ---------------------
    //   Присваивание
    // ---------------------
    public override void ExitAssignment([NotNull] AbaScriptParser.AssignmentContext context)
    {
        // Тип выражения, присваиваемого справа
        var assignedExprType = typeStack.Pop();

        // Для упрощения предполагаем, что слева либо ID, либо fieldAccess
        // Если это ID[...] или просто ID
        var varNameToken = context.ID();
        if (varNameToken != null)
        {
            var varName = varNameToken.GetText();
            if (!symbolTable.ContainsKey(varName))
            {
                Console.Error.WriteLine($"Ошибка: переменная {varName} не объявлена");
                return;
            }

            var varType = symbolTable[varName];
            if (!TypeHelper.AreCompatible(varType, assignedExprType))
            {
                Console.Error.WriteLine($"Ошибка: нельзя присвоить {varName} типа {varType} " +
                                        $"значение типа {assignedExprType}");
            }
        }
        else
        {
            // Иначе это fieldAccess: ID '.' ID
            // Например, obj.field = expr;
            // Для упрощения рассмотрим только один уровень: obj.field
            var fa = context.fieldAccess();
            if (fa != null)
            {
                string leftObjectName = fa.ID(0).GetText();   // объект
                string fieldName = fa.ID(1).GetText();       // поле

                if (!symbolTable.TryGetValue(leftObjectName, out string objType))
                {
                    Console.Error.WriteLine($"Ошибка: переменная (объект) {leftObjectName} не объявлена");
                    return;
                }
                if (!TypeHelper.IsClassType(objType))
                {
                    Console.Error.WriteLine($"Ошибка: {leftObjectName} не является объектом класса ({objType})");
                    return;
                }
                // Проверяем есть ли такой класс
                if (!classTable.TryGetValue(objType, out var fields))
                {
                    Console.Error.WriteLine($"Ошибка: класс {objType} не объявлен");
                    return;
                }
                // Проверяем, есть ли поле в классе
                if (!fields.TryGetValue(fieldName, out var fieldType))
                {
                    Console.Error.WriteLine($"Ошибка: в классе {objType} нет поля {fieldName}");
                    return;
                }
                // Сопоставляем тип
                if (!TypeHelper.AreCompatible(fieldType, assignedExprType))
                {
                    Console.Error.WriteLine($"Ошибка: поле {fieldName} класса {objType} " +
                                            $"имеет тип {fieldType}, а присваивается {assignedExprType}");
                }
            }
        }
    }

    // ---------------------
    //    Объявление класса
    // ---------------------
    public override void EnterClassDef([NotNull] AbaScriptParser.ClassDefContext context)
    {
        // Начинаем новый "профиль" для полей класса
        var className = context.ID().GetText();
        if (!classTable.ContainsKey(className))
        {
            classTable[className] = new Dictionary<string, string>();
        }
    }

    // Можно реализовать логику добавления полей в ExitClassDef, 
    // предварительно считав их из переменных внутри classMember.

    // ---------------------
    //   Вызов функции, метод
    // ---------------------
    // (Простой пример, если есть funcCall в factor)
    public override void ExitFuncCall([NotNull] AbaScriptParser.FuncCallContext context)
    {
        // Допустим, мы пока что считаем, что любая функция возвращает int (заглушка)
        // Или, при более сложном проекте, нужно отдельно хранить инфу о функциях, их сигнатурах и возвращаемых типах
        // Для демонстрации просто возвращаем "int" (или "unknown").
        // Снимаем из стека типы аргументов (если нужно), не забывая, что их столько, сколько expr в скобках

        var argCount = context.expr().Length;
        for (int i = 0; i < argCount; i++)
            typeStack.Pop(); // Снимаем тип каждого аргумента

        // Пушим результат вызова
        typeStack.Push("int"); // или другой логикой из таблицы функций
    }

    // ---------------------
    //   factor (различные #метки)
    // ---------------------

    // Унарный минус
    public override void ExitUnaryMinus([NotNull] AbaScriptParser.UnaryMinusContext context)
    {
        var innerType = typeStack.Pop();
        // Проверяем, что innerType - int
        if (innerType != "int")
        {
            Console.Error.WriteLine($"Ошибка: унарный минус применим только к int, получен {innerType}");
            typeStack.Push("unknown");
        }
        else
        {
            typeStack.Push("int");
        }
    }

    // Скобки
    public override void ExitParens([NotNull] AbaScriptParser.ParensContext context)
    {
        // Ничего особого: тип уже лежит в стеке (последним перед вызовом ExitParens).
        // Но в момент выхода из Parens правило забирает этот тип и снова кладёт.
        // Фактически можно ничего не делать, если мы уже используем pop в других местах.
        // Здесь пример для иллюстрации:
        var innerType = typeStack.Pop();
        typeStack.Push(innerType);
    }

    // Число
    public override void ExitNumber([NotNull] AbaScriptParser.NumberContext context)
    {
        typeStack.Push("int");
    }

    // Строка
    public override void ExitString([NotNull] AbaScriptParser.StringContext context)
    {
        typeStack.Push("string");
    }

    // Доступ к переменной или массиву: ID ('[' expr ']')?
    public override void ExitVariableOrArrayAccess([NotNull] AbaScriptParser.VariableOrArrayAccessContext context)
    {
        var varName = context.ID().GetText();
        if (!symbolTable.TryGetValue(varName, out string varType))
        {
            Console.Error.WriteLine($"Ошибка: переменная {varName} не объявлена.");
            typeStack.Push("unknown");
        }
        else
        {
            // Если есть [expr], можно проверить, действительно ли varType - массив.
            // Для упрощения пусть любой int[] в грамматике так и называется "int[]", и т.д.
            // Здесь пропустим детали. Просто возвращаем базовый varType:
            typeStack.Push(varType);
        }
    }

    // Вызов метода (как отдельный label MethCall)
    public override void ExitMethodCall([NotNull] AbaScriptParser.MethodCallContext context)
    {
        // Аналогично ExitMethodCall
        var argCount = context.expr().Length;
        for (int i = 0; i < argCount; i++)
            typeStack.Pop();

        typeStack.Push("int");
    }

    public override void ExitFieldAccess([NotNull] AbaScriptParser.FieldAccessContext context)
    {
        string leftObjectName = context.ID(0).GetText(); // obj
        string fieldName = context.ID(1).GetText();      // field

        // Сначала узнаём тип obj
        if (!symbolTable.TryGetValue(leftObjectName, out string objType))
        {
            Console.Error.WriteLine($"Ошибка: переменная (объект) {leftObjectName} не объявлена");
            typeStack.Push("unknown");
            return;
        }
        if (!TypeHelper.IsClassType(objType))
        {
            Console.Error.WriteLine($"Ошибка: {leftObjectName} не является объектом класса ({objType})");
            typeStack.Push("unknown");
            return;
        }
        if (!classTable.TryGetValue(objType, out var fields))
        {
            Console.Error.WriteLine($"Ошибка: класс {objType} не объявлен");
            typeStack.Push("unknown");
            return;
        }
        if (!fields.TryGetValue(fieldName, out var fieldType))
        {
            Console.Error.WriteLine($"Ошибка: в классе {objType} нет поля {fieldName}");
            typeStack.Push("unknown");
            return;
        }

        // Успешно нашли поле: пушим его тип
        typeStack.Push(fieldType);
    }

    // Создание объекта класса: 'new' ID
    public override void ExitNewClass([NotNull] AbaScriptParser.NewClassContext context)
    {
        // Пример: new MyClass => тип выражения "MyClass"
        var className = context.ID().GetText();

        // Проверим, что класс вообще объявлен
        if (!classTable.ContainsKey(className))
        {
            Console.Error.WriteLine($"Ошибка: класс {className} не объявлен");
            typeStack.Push("unknown");
            return;
        }
        typeStack.Push(className);
    }

    public override void ExitAddSub([NotNull] AbaScriptParser.AddSubContext context)
    {
        var right = typeStack.Pop();
        var left = typeStack.Pop();
        if (left != "int" || right != "int")
        {
            Console.Error.WriteLine("Ошибка: операция + или - применима только к int.");
            typeStack.Push("unknown");
        }
        else
        {
            typeStack.Push("int");
        }
    }

    public override void ExitMulDivMod([NotNull] AbaScriptParser.MulDivModContext context)
    {
        var right = typeStack.Pop();
        var left = typeStack.Pop();
        if (left != "int" || right != "int")
        {
            Console.Error.WriteLine("Ошибка: операция *, /, % применима только к int.");
            typeStack.Push("unknown");
        }
        else
        {
            typeStack.Push("int");
        }
    }
}