using AbaScript.AntlrClasses;
using AbaScript.AntlrClasses.Models;

namespace AbaScript.InterpreterClasses;

public partial class AbaScriptInterpreter
{
    public override object VisitAddSub(AbaScriptParser.AddSubContext context)
    {
        var left = Visit(context.expr());
        var right = Visit(context.term());
        var operatorText = context.GetChild(1).GetText();

        logger.Log($"left={left}, right={right}, leftType={left?.GetType()}, rightType={right?.GetType()}");

        return operatorText switch
        {
            "+" => Add(left, right),
            "-" => Subtract(left, right),
            _ => throw new InvalidOperationException("Unsupported operation")
        };
    }

    public override object VisitMulDivMod(AbaScriptParser.MulDivModContext context)
    {
        var left = Visit(context.term());
        var right = Visit(context.factor());
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

    private object Add(object left, object right)
    {
        if (left is int leftInt && right is int rightInt)
            return leftInt + rightInt;
        if (left is string leftStr && right is string rightStr)
            return leftStr + rightStr;

        throw new InvalidOperationException($"Несовместимые типы для операции '+': {left}, {right}");
    }

    private object Subtract(object left, object right)
    {
        if (left is int leftInt && right is int rightInt)
            return leftInt - rightInt;

        throw new InvalidOperationException($"Несовместимые типы для операции '-': {left}, {right}");
    }

    private object Multiply(object left, object right)
    {
        if (left is int leftInt && right is int rightInt)
            return leftInt * rightInt;

        throw new InvalidOperationException($"Несовместимые типы для операции '*': {left}, {right}");
    }

    private object Divide(object left, object right)
    {
        if (left is int leftInt && right is int rightInt)
        {
            if (rightInt == 0)
                throw new DivideByZeroException("Деление на ноль невозможно.");
            return leftInt / rightInt;
        }

        throw new InvalidOperationException($"Несовместимые типы для операции '/': {left}, {right}");
    }

    private object Modulus(object left, object right)
    {
        if (left is int leftInt && right is int rightInt)
        {
            if (rightInt == 0)
                throw new DivideByZeroException("Деление на ноль невозможно.");
            return leftInt % rightInt;
        }

        throw new InvalidOperationException($"Несовместимые типы для операции '%': {left}, {right}");
    }

    public override object VisitUnaryMinus(AbaScriptParser.UnaryMinusContext context)
    {
        var value = Visit(context.factor());
        if (value is int intValue)
            return -intValue;

        throw new InvalidOperationException($"Несовместимый тип для унарного минуса: {value}");
    }

    public override object VisitFactor(AbaScriptParser.FactorContext context)
    {
        if (context is AbaScriptParser.NewClassContext newClassContext)
        {
            var className = newClassContext.ID().GetText();
            // Создаём экземпляр класса (концептуально)
            if (!classDefinitions.ContainsKey(className))
                throw new InvalidOperationException($"Класс '{className}' не определен.");

            var newInstance = new ClassInstance();
            // Инициализируем поля дефолтными значениями
            // (в зависимости от того, что нужно вашей логике)
            logger.Log($"Создан новый объект класса {className}.");

            // Возвращаем сам объект, чтобы его можно было присвоить
            return newInstance;
        }

        // Остальные варианты factor
        return context switch
        {
            AbaScriptParser.UnaryMinusContext unaryMinusContext => VisitUnaryMinus(unaryMinusContext),
            AbaScriptParser.ParensContext parensContext => Visit(parensContext.expr()),
            AbaScriptParser.NumberContext numberContext => VisitNumber(numberContext),
            AbaScriptParser.StringContext stringContext => VisitString(stringContext),
            AbaScriptParser.VariableOrArrayAccessContext varContext => VisitVariableOrArrayAccess(varContext),
            AbaScriptParser.FunctionalCallContext funcCallContext => VisitFuncCall(funcCallContext.funcCall()),
            _ => throw new InvalidOperationException("Unsupported factor type")
        };
    }

    public override object VisitNumber(AbaScriptParser.NumberContext context)
    {
        if (int.TryParse(context.GetText(), out var number))
            return number;
        throw new InvalidOperationException($"Невозможно преобразовать в число: {context.GetText()}");
    }

    public override object VisitString(AbaScriptParser.StringContext context)
    {
        return context.GetText().Trim('"');
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
}