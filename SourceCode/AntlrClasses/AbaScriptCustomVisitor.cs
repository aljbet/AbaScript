﻿namespace AbaScript.AntlrClasses;

public class AbaScriptCustomVisitor : AbaScriptBaseVisitor<object>
{
    private readonly Dictionary<string, object> variables = new();
    private readonly Dictionary<string, (List<string> Parameters, AbaScriptParser.BlockContext Body)> functions = new();
    private readonly Logger logger = new();

    public override object VisitVariableDeclaration(AbaScriptParser.VariableDeclarationContext context)
    {
        var varName = context.ID().GetText();
        object value = null;

        if (context.NUMBER() != null)
        {
            // Array declaration with a specified size
            int size = int.Parse(context.NUMBER().GetText());
            value = new object[size];
        }
        else if (context.expr() != null)
        {
            // Regular variable initialization
            value = Visit(context.expr());
        }

        variables[varName] = value;
        logger.Log($"Переменная {varName} объявлена со значением: {value} (тип: {GetType(value)})");
        return null;
    }

    
    public override object VisitVariableOrArrayAccess(AbaScriptParser.VariableOrArrayAccessContext context)
    {
        string variableName = context.ID().GetText();
        object value;

        if (context.expr() != null)
        {
            var index = (int)Visit(context.expr());
            var array = (object[])variables[variableName];
            value = array[index];
        }
        else
        {
            if (!variables.TryGetValue(variableName, out value))
                throw new InvalidOperationException($"Переменная '{variableName}' не объявлена.");
        }

        return value;
    }
    public override object VisitAssignment(AbaScriptParser.AssignmentContext context)
    {
        var varName = context.ID().GetText();

        if (context.expr() != null)
        {
            object value;
            if (context.expr().Length == 2)
            {
                value = Visit(context.expr(1));
                var index = (int)Visit(context.expr(0));
                var array = (object[])variables[varName];
                array[index] = value;
            }
            else
            {
                value = Visit(context.expr(0));
                variables[varName] = value;
            }
            logger.Log($"Переменная {varName} обновлена: {value} (тип: {GetType(value)})");
        }
        
        return null;
    }

    public override object VisitAddSub(AbaScriptParser.AddSubContext context)
    {
        var left = Visit(context.expr());
        var right = Visit(context.term());

        // Determine the operator by checking the text of the middle child
        var operatorText = context.GetChild(1).GetText();

        logger.Log($"left={left}, right={right}, leftType={left?.GetType()}, rightType={right?.GetType()}");

        return operatorText switch
        {
            "+" => Add(left, right),
            "-" => Subtract(left, right),
            _ => throw new InvalidOperationException("Unsupported operation")
        };
    }

    private object Add(object left, object right)
    {
        if (left is int leftInt && right is int rightInt)
        {
            return leftInt + rightInt;
        }
        if (left is string leftStr && right is string rightStr)
        {
            return leftStr + rightStr;
        }
        throw new InvalidOperationException($"Несовместимые типы для операции '+': {left}, {right}");
    }

    private object Subtract(object left, object right)
    {
        if (left is int leftInt && right is int rightInt)
        {
            return leftInt - rightInt;
        }
        throw new InvalidOperationException($"Несовместимые типы для операции '-': {left}, {right}");
    }
    
    public override object VisitMulDivMod(AbaScriptParser.MulDivModContext context)
    {
        var left = Visit(context.term());
        var right = Visit(context.factor());

        // Determine the operator by checking the text of the middle child
        var operatorText = context.GetChild(1).GetText();

        logger.Log($"left={left}, right={right}, leftType={left?.GetType()}, rightType={right?.GetType()}");

        return operatorText switch
        {
            "*" => Multiply(left, right),
            "/" => Divide(left, right),
            "%" => Modulus(left, right),
            _ => throw new InvalidOperationException("Unsupported operation")
        };
    }

    private object Multiply(object left, object right)
    {
        if (left is int leftInt && right is int rightInt)
        {
            return leftInt * rightInt;
        }
        throw new InvalidOperationException($"Несовместимые типы для операции '*': {left}, {right}");
    }

    private object Divide(object left, object right)
    {
        if (left is int leftInt && right is int rightInt)
        {
            if (rightInt == 0)
            {
                throw new DivideByZeroException("Деление на ноль невозможно.");
            }
            return leftInt / rightInt;
        }
        throw new InvalidOperationException($"Несовместимые типы для операции '/': {left}, {right}");
    }

    private object Modulus(object left, object right)
    {
        if (left is int leftInt && right is int rightInt)
        {
            if (rightInt == 0)
            {
                throw new DivideByZeroException("Деление на ноль невозможно.");
            }
            return leftInt % rightInt;
        }
        throw new InvalidOperationException($"Несовместимые типы для операции '%': {left}, {right}");
    }
    
    public override object VisitInputStatement(AbaScriptParser.InputStatementContext context)
    {
        var varName = context.ID().GetText();
        Console.Write($"Введите значение для {varName}: ");
        var input = Console.ReadLine();

        if (context.expr() != null)
        {
            var index = (int)Visit(context.expr());
            var array = (object[])variables[varName];
            array[index] = TryParseNumber(input);
        }
        else
        {
            variables[varName] = TryParseNumber(input);
        }

        return null;
    }

    private object TryParseNumber(string input)
    {
        if (int.TryParse(input, out var number))
        {
            return number;
        }
        return input; // Return as string if not a number
    }
    public override object VisitIfStatement(AbaScriptParser.IfStatementContext context)
    {
        for (int i = 0; i < context.logicalExpr().Length; i++)
        {
            var conditionResult = (bool)Visit(context.logicalExpr(i));
            if (conditionResult)
            {
                Visit(context.block(i));
                return null;
            }
        }

        if (context.block().Length > context.logicalExpr().Length)
        {
            Visit(context.block(context.logicalExpr().Length)); // Блок else
        }

        return null;
    }
    
    public override object VisitLogicalExpr(AbaScriptParser.LogicalExprContext context)
    {
        switch (context)
        {
            case AbaScriptParser.AndExprContext andExpr:
            {
                var left = (bool)Visit(andExpr.logicalExpr());
                var right = (bool)Visit(andExpr.condition());
                return left && right;
            }
            case AbaScriptParser.OrExprContext orExpr:
            {
                var left = (bool)Visit(orExpr.logicalExpr());
                var right = (bool)Visit(orExpr.condition());
                return left || right;
            }
            case AbaScriptParser.ConditionExprContext conditionExpr:
                return Visit(conditionExpr.condition());
            default:
                throw new InvalidOperationException("Unsupported logical expression");
        }
    }
    
    public override object VisitCondition(AbaScriptParser.ConditionContext context)
    {
        var left = Visit(context.expr(0));
        var right = Visit(context.expr(1));
        var operatorText = context.comparisonOp().GetText();

        return operatorText switch
        {
            "==" => Equals(left, right),
            "!=" => !Equals(left, right),
            "<" => (int)left < (int)right,
            "<=" => (int)left <= (int)right,
            ">" => (int)left > (int)right,
            ">=" => (int)left >= (int)right,
            _ => throw new InvalidOperationException("Unsupported comparison operation")
        };
    }

    public override object VisitNumber(AbaScriptParser.NumberContext context)
    {
        if (int.TryParse(context.GetText(), out var number))
        {
            return number;
        }
        throw new InvalidOperationException($"Невозможно преобразовать в число: {context.GetText()}");
    }

    public override object VisitString(AbaScriptParser.StringContext context)
    {
        return context.GetText().Trim('"'); // Убираем кавычки
    }

    public override object VisitOutputStatement(AbaScriptParser.OutputStatementContext context)
    {
        var value = Visit(context.expr());
        Console.WriteLine(value);
        return null;
    }
    
    public override object VisitForStatement(AbaScriptParser.ForStatementContext context)
    {
        if (context.variableDeclaration() != null)
        {
            Visit(context.variableDeclaration());
        }
        else if (context.assignment(0) != null)
        {
            Visit(context.assignment(0));
        }

        while (true)
        {
            // Evaluate the loop condition
            if (context.logicalExpr() != null)
            {
                var conditionResult = Visit(context.logicalExpr());
                if (conditionResult is not bool boolResult)
                {
                    throw new InvalidOperationException("The condition must evaluate to a boolean value.");
                }
                if (!boolResult)
                {
                    break;
                }
            }

            // Execute the loop body
            try
            {
                Visit(context.block());
            }
            catch (BreakException)
            {
                break;
            }
            catch (ContinueException)
            {
                // Perform the increment step before continuing
                if (context.assignment(1) != null)
                {
                    Visit(context.assignment(1));
                }
                continue;
            }

            // Perform the increment step
            if (context.variableDeclaration() == null && context.assignment(1) != null)
            {
                Visit(context.assignment(1));
            }
            else if (context.assignment(0) != null)
            {
                Visit(context.assignment(0));
            }
        }
        return null;
    }

    public override object VisitWhileStatement(AbaScriptParser.WhileStatementContext context)
    {
        while (true)
        {
            var conditionResult = Visit(context.logicalExpr());
            if (conditionResult is not bool boolResult)
            {
                throw new InvalidOperationException("The condition must evaluate to a boolean value.");
            }
            if (!boolResult)
            {
                break;
            }

            try
            {
                Visit(context.block());
            }
            catch (BreakException)
            {
                break;
            }
            catch (ContinueException)
            {
                continue;
            }
        }
        return null;
    }
    public override object VisitBreakStatement(AbaScriptParser.BreakStatementContext context)
    {
        throw new BreakException();
    }

    public override object VisitContinueStatement(AbaScriptParser.ContinueStatementContext context)
    {
        throw new ContinueException();
    }
    public override object VisitFunctionDef(AbaScriptParser.FunctionDefContext context)
    {
        var funcName = context.ID(0).GetText();
        var parameters = context.ID().Skip(1).Select(p => p.GetText()).ToList(); // Параметры функции
        functions[funcName] = (parameters, context.block());
        logger.Log($"Функция {funcName} определена с параметрами: {string.Join(", ", parameters)}");
        return null;
    }
    
    public override object VisitFuncCall(AbaScriptParser.FuncCallContext context)
    {
        var funcName = context.ID().GetText();

        if (!functions.TryGetValue(funcName, out var functionInfo))
            throw new InvalidOperationException($"Функция '{funcName}' не определена.");

        var arguments = context.expr().Select(expr => Visit(expr)).ToList();

        if (arguments.Count != functionInfo.Parameters.Count)
            throw new InvalidOperationException($"Количество аргументов не совпадает для функции '{funcName}'.");

        // Сохраняем текущие переменные
        var oldVariables = new Dictionary<string, object>(variables);

        // Создаём локальный контекст переменных
        variables.Clear();
        for (int i = 0; i < arguments.Count; i++)
            variables[functionInfo.Parameters[i]] = arguments[i];

        try
        {
            Visit(functionInfo.Body);
        }
        catch (ReturnException ex)
        {
            return ex.ReturnValue; // Возвращаем значение из функции
        }
        finally
        {
            // Восстанавливаем глобальные переменные
            variables.Clear();
            foreach (var kvp in oldVariables)
                variables[kvp.Key] = kvp.Value;
        }

        return null; // Если return не вызван
    }
    
    public override object VisitReturnStatement(AbaScriptParser.ReturnStatementContext context)
    {
        var returnValue = Visit(context.expr());
        throw new ReturnException(returnValue);
    }
    
    public override object VisitFactor(AbaScriptParser.FactorContext context)
    {
        return context switch
        {
            AbaScriptParser.UnaryMinusContext unaryMinusContext => VisitUnaryMinus(unaryMinusContext),
            AbaScriptParser.ParensContext parensContext => Visit(parensContext.expr()),
            AbaScriptParser.NumberContext numberContext => VisitNumber(numberContext),
            AbaScriptParser.StringContext stringContext => VisitString(stringContext),
            AbaScriptParser.VariableOrArrayAccessContext VariableOrArrayAccessContext => VisitVariableOrArrayAccess(
                VariableOrArrayAccessContext),
            AbaScriptParser.FunctionalCallContext funcCallContext => VisitFuncCall(funcCallContext.funcCall()),
            _ => throw new InvalidOperationException("Unsupported factor type")
        };
    }
    
    public override object VisitUnaryMinus(AbaScriptParser.UnaryMinusContext context)
    {
        var value = Visit(context.factor());

        if (value is int intValue)
        {
            return -intValue;
        }

        throw new InvalidOperationException($"Несовместимый тип для унарного минуса: {value}");
    }


    
    private string GetType(object value)
    {
        return value switch
        {
            int => "INT",
            string => "STRING",
            _ => "UNKNOWN"
        };
    }
}