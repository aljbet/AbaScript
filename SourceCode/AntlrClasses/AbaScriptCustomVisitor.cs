using AbaScript.AntlrClasses.Models;

namespace AbaScript.AntlrClasses;

public class AbaScriptCustomVisitor : AbaScriptBaseVisitor<object>
{
    private readonly Dictionary<string, ClassDefinition> classDefinitions = new();
    private readonly Dictionary<string, ClassInstance> classInstances = new();
    private readonly Dictionary<string, Variable> variables = new();

    private readonly
        Dictionary<string, (List<(string type, string name)> Parameters, string ReturnType, AbaScriptParser.BlockContext
            Body)> functions = new();

    private readonly Logger logger = new Logger();

    public override object VisitVariableDeclaration(AbaScriptParser.VariableDeclarationContext context)
    {
        var varType = context.type().GetText();
        var varName = context.ID().GetText();
        object value = null;

        if (context.NUMBER() != null)
        {
            int size = int.Parse(context.NUMBER().GetText());
            value = new object[size];
        }
        else if (context.expr() != null)
        {
            value = Visit(context.expr());
            if (!CheckType(varType, value))
                throw new InvalidOperationException($"Переменная {varName} должна быть типа {varType}.");
        }

        var variableType = Enum.Parse<VariableType>(varType, true);
        variables[varName] = new Variable(variableType, value);
        logger.Log($"Переменная {varName} объявлена со значением: {value} (тип: {varType})");
        return null;
    }

    public override object VisitVariableOrArrayAccess(AbaScriptParser.VariableOrArrayAccessContext context)
    {
        string variableName = context.ID().GetText();
        object value;

        if (context.expr() != null)
        {
            var index = (int)Visit(context.expr());
            if (!variables.TryGetValue(variableName, out var variable) || !(variable.Value is object[] array))
                throw new InvalidOperationException(
                    $"Переменная '{variableName}' не объявлена или не является массивом.");
            value = array[index];
        }
        else
        {
            if (!variables.TryGetValue(variableName, out var variable))
                throw new InvalidOperationException($"Переменная '{variableName}' не объявлена.");
            value = variable.Value;
        }

        return value;
    }

    public override object VisitAssignment(AbaScriptParser.AssignmentContext context)
    {
        var expressions = context.expr();

        if (context.fieldAccess() != null)
        {
            // Handle field access assignment
            var instanceName = context.fieldAccess().ID(0).GetText();
            var fieldName = context.fieldAccess().ID(1).GetText();

            if (!classInstances.TryGetValue(instanceName, out var instance))
                throw new InvalidOperationException($"Экземпляр '{instanceName}' не существует.");

            var className = instance.GetType().Name;
            if (!classDefinitions[className].Fields.ContainsKey(fieldName))
                throw new InvalidOperationException($"Поле '{fieldName}' не определено в классе '{className}'.");

            object value = Visit(expressions[0]);
            instance.Fields[fieldName] = value;
            logger.Log($"Поле {fieldName} экземпляра {instanceName} обновлено: {value}");
        }
        else
        {
            // Handle variable or array assignment
            var varName = context.ID().GetText();

            if (expressions.Length == 2)
            {
                var index = (int)Visit(expressions[0]);
                if (!variables.TryGetValue(varName, out var variable) || !(variable.Value is object[] array))
                    throw new InvalidOperationException(
                        $"Переменная '{varName}' не является массивом.");

                object value = Visit(expressions[1]);
                array[index] = value;
            }
            else
            {
                object value = Visit(expressions[0]);
                if (!variables.TryGetValue(varName, out var variable))
                    throw new InvalidOperationException($"Переменная '{varName}' не объявлена.");

                if (!CheckType(variable.Type.ToString(), value))
                    throw new InvalidOperationException($"Переменная {varName} должна быть типа {variable.Type}.");
                variable.Value = value;
                logger.Log($"Переменная {varName} обновлена: {value} (тип: {variable.Type})");
            }
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
            if (!variables.TryGetValue(varName, out var variable) || !(variable.Value is object[] array))
                throw new InvalidOperationException($"Переменная '{varName}' не объявлена или не является массивом.");
            array[index] = TryParseNumber(input);
        }
        else
        {
            if (!variables.TryGetValue(varName, out var variable))
                throw new InvalidOperationException($"Переменная '{varName}' не объявлена.");
            variable.Value = TryParseNumber(input);
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
            if (context.assignment(1) != null)
            {
                Visit(context.assignment(1));
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
        var funcName = context.ID().GetText();
        var returnType = context.returnType().GetText();
        var parameters = context.typedParam().Select(p => (p.type().GetText(), p.ID().GetText())).ToList();

        functions[funcName] = (parameters, returnType, context.block());
        logger.Log(
            $"Функция {funcName} определена с параметрами: {string.Join(", ", parameters.Select(p => $"{p.Item1} {p.Item2}"))}");
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

        for (int i = 0; i < arguments.Count; i++)
        {
            var expectedType = functionInfo.Parameters[i].type;
            if (!CheckType(expectedType, arguments[i]))
                throw new InvalidOperationException(
                    $"Аргумент {i} функции {funcName} должен быть типа {expectedType}.");
        }

        var oldVariables = new Dictionary<string, Variable>(variables);

        variables.Clear();
        for (int i = 0; i < arguments.Count; i++)
        {
            var parameterType = Enum.Parse<VariableType>(functionInfo.Parameters[i].type, true);
            variables[functionInfo.Parameters[i].name] = new Variable(parameterType, arguments[i]);
        }

        try
        {
            Visit(functionInfo.Body);
        }
        catch (ReturnException ex)
        {
            if (!CheckType(functionInfo.ReturnType, ex.ReturnValue))
                throw new InvalidOperationException(
                    $"Возвращаемое значение функции {funcName} должно быть типа {functionInfo.ReturnType}.");
            return ex.ReturnValue;
        }
        finally
        {
            variables.Clear();
            foreach (var kvp in oldVariables)
                variables[kvp.Key] = kvp.Value;
        }

        return null;
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

    public override object VisitClassDef(AbaScriptParser.ClassDefContext context)
    {
        var className = context.ID().GetText();
        var classDef = new ClassDefinition();

        foreach (var member in context.classMember())
        {
            if (member.variableDeclaration() != null)
            {
                var varType = member.variableDeclaration().type().GetText();
                var varName = member.variableDeclaration().ID().GetText();
                classDef.Fields[varName] = Enum.Parse<VariableType>(varType, true);
            }
            else if (member.functionDef() != null)
            {
                var funcName = member.functionDef().ID().GetText();
                var returnType = member.functionDef().returnType().GetText();
                var parameters = member.functionDef().typedParam().Select(p => (p.type().GetText(), p.ID().GetText()))
                    .ToList();
                classDef.Methods[funcName] = (parameters, returnType, member.functionDef().block());
            }
        }

        classDefinitions[className] = classDef;
        logger.Log($"Класс {className} определен.");
        return null;
    }

    public override object VisitClassInstantiation(AbaScriptParser.ClassInstantiationContext context)
    {
        var className = context.ID().GetText();
        if (!classDefinitions.ContainsKey(className))
            throw new InvalidOperationException($"Класс '{className}' не определен.");

        var instanceName = context.ID().GetText();
        classInstances[instanceName] = new ClassInstance();
        logger.Log($"Экземпляр класса {className} создан как {instanceName}.");
        return null;
    }

    public override object VisitMethodCall(AbaScriptParser.MethodCallContext context)
    {
        var instanceName = context.ID(0).GetText();
        var methodName = context.ID(1).GetText();

        if (!classInstances.TryGetValue(instanceName, out var instance))
            throw new InvalidOperationException($"Экземпляр '{instanceName}' не существует.");

        var className = classInstances[instanceName].GetType().Name;
        if (!classDefinitions[className].Methods.TryGetValue(methodName, out var methodInfo))
            throw new InvalidOperationException($"Метод '{methodName}' не определен в классе '{className}'.");

        var arguments = context.expr().Select(expr => Visit(expr)).ToList();

        if (arguments.Count != methodInfo.Parameters.Count)
            throw new InvalidOperationException($"Количество аргументов не совпадает для метода '{methodName}'.");

        for (int i = 0; i < arguments.Count; i++)
        {
            var expectedType = methodInfo.Parameters[i].type;
            if (!CheckType(expectedType, arguments[i]))
                throw new InvalidOperationException(
                    $"Аргумент {i} метода {methodName} должен быть типа {expectedType}.");
        }

        var oldVariables = new Dictionary<string, Variable>(variables);

        variables.Clear();
        for (int i = 0; i < arguments.Count; i++)
        {
            var parameterType = Enum.Parse<VariableType>(methodInfo.Parameters[i].type, true);
            variables[methodInfo.Parameters[i].name] = new Variable(parameterType, arguments[i]);
        }

        try
        {
            Visit(methodInfo.Body);
        }
        catch (ReturnException ex)
        {
            if (!CheckType(methodInfo.ReturnType, ex.ReturnValue))
                throw new InvalidOperationException(
                    $"Возвращаемое значение метода {methodName} должно быть типа {methodInfo.ReturnType}.");
            return ex.ReturnValue;
        }
        finally
        {
            variables.Clear();
            foreach (var kvp in oldVariables)
                variables[kvp.Key] = kvp.Value;
        }

        return null;
    }

    private static bool CheckType(string type, object value)
    {
        return Enum.TryParse(type, true, out VariableType variableType) && variableType switch
        {
            VariableType.Int => value is int,
            VariableType.String => value is string,
            VariableType.Array => value is object[],
            _ => false,
        };
    }
}