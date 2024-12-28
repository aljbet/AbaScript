using System.Text;
using AbaScript.AntlrClasses;
using LLVMSharp.Interop;

namespace AbaScript.LlvmClasses;

public class LlvmVisitor : AbaScriptBaseVisitor<object>
{
    private readonly Logger logger = new();
    private readonly Stack<LLVMValueRef> valueStack = new();
    private readonly Dictionary<string, LLVMValueRef> variables = new();

    public override object VisitVariableDeclaration(AbaScriptParser.VariableDeclarationContext context)
    {
        var varType = context.type().GetText();
        var varName = context.ID().GetText();
        LLVMValueRef value = null;

        if (context.expr() != null)
        {
            Visit(context.expr());
            value = valueStack.Pop();
            if (!CheckType(varType, value.TypeOf.Kind))
                throw new InvalidOperationException($"Переменная {varName} должна быть типа {varType}.");
        }

        variables[varName] = value;
        logger.Log($"Переменная {varName} объявлена со значением: {value} (тип: {varType})");

        return context;
    }

    public override object VisitVariableOrArrayAccess(AbaScriptParser.VariableOrArrayAccessContext context)
    {
        string variableName = context.ID().GetText();
        if (variables.TryGetValue(variableName, out var value))
        {
            valueStack.Push(value);
        }
        else
        {
            throw new InvalidOperationException($"Переменная '{variableName}' не объявлена.");
        }

        return context;
    }

    public override unsafe object VisitInputStatement(AbaScriptParser.InputStatementContext context)
    {
        var varName = context.ID().GetText();
        Console.Write($"Введите значение для {varName}: ");
        var input = Console.ReadLine();

        if (!variables.ContainsKey(varName))
            throw new InvalidOperationException($"Переменная '{varName}' не объявлена.");
        var value = TryParseNumber(input);
        switch (value)
        {
            case int valueInt:
                variables[varName] = LLVM.ConstInt(LLVM.IntType(32), (ulong)valueInt, 1);
                break;
            case string valueStr:
                var bytes = Encoding.Default.GetBytes(valueStr);

                fixed (byte* p = bytes)
                {
                    sbyte* sp = (sbyte*)p;
                    variables[varName] = LLVM.ConstString(sp, (uint)valueStr.Length, 0);
                }
                break;
        }

        return context;
    }

    private object TryParseNumber(string input)
    {
        if (int.TryParse(input, out var number))
        {
            return number;
        }

        return input; // Return as string if not a number
    }

    public override unsafe object VisitOutputStatement(AbaScriptParser.OutputStatementContext context)
    {
        Visit(context.expr());
        var currentElement = valueStack.Pop();

        switch (currentElement.TypeOf.Kind)
        {
            case LLVMTypeKind.LLVMIntegerTypeKind:
                Console.WriteLine(LLVM.ConstIntGetSExtValue((LLVMOpaqueValue*)currentElement.Handle));
                break;
            case LLVMTypeKind.LLVMArrayTypeKind:
                var asString = currentElement.ToString();
                var index = asString.IndexOf(']');
                Console.WriteLine(asString.Substring(index + 4, asString.Length - index - 8));
                break;
        }

        return context;
    }

    public override unsafe object VisitNumber(AbaScriptParser.NumberContext context)
    {
        if (int.TryParse(context.GetText(), out var number))
        {
            valueStack.Push(LLVM.ConstInt(LLVM.IntType(32), (ulong)number, 1));
            return context;
        }

        throw new InvalidOperationException($"Невозможно преобразовать в число: {context.GetText()}");
    }

    public override unsafe object VisitString(AbaScriptParser.StringContext context)
    {
        var str = context.GetText().Trim('"');
        var bytes = Encoding.Default.GetBytes(str);

        fixed (byte* p = bytes)
        {
            sbyte* sp = (sbyte*)p;
            valueStack.Push(LLVM.ConstString(sp, (uint)str.Length, 0));
        }

        return context;
    }

    public override unsafe object VisitAddSub(AbaScriptParser.AddSubContext context)
    {
        Visit(context.expr());
        Visit(context.term());

        var right = valueStack.Pop();
        var left = valueStack.Pop();

        // Determine the operator by checking the text of the middle child
        var operatorText = context.GetChild(1).GetText();

        switch (right.TypeOf.Kind)
        {
            case LLVMTypeKind.LLVMIntegerTypeKind:
                if (left.TypeOf.Kind != LLVMTypeKind.LLVMIntegerTypeKind)
                {
                    throw new InvalidOperationException("Incompatible types.");
                }

                var rightValue = LLVM.ConstIntGetSExtValue((LLVMOpaqueValue*)right.Handle);
                var leftValue = LLVM.ConstIntGetSExtValue((LLVMOpaqueValue*)left.Handle);
                switch (operatorText)
                {
                    case "+":
                        valueStack.Push(
                            LLVM.ConstInt(LLVM.IntType(32), (ulong)(leftValue + rightValue), 1)
                        );
                        break;
                    case "-":
                        valueStack.Push(
                            LLVM.ConstInt(LLVM.IntType(32), (ulong)(leftValue - rightValue), 1)
                        );
                        break;
                    default:
                        throw new InvalidOperationException("Unsupported operation");
                }

                break;
            case LLVMTypeKind.LLVMArrayTypeKind:
                if (left.TypeOf.Kind != LLVMTypeKind.LLVMArrayTypeKind)
                {
                    throw new InvalidOperationException("Incompatible types.");
                }

                var leftAsString = left.ToString();
                var leftIndex = leftAsString.IndexOf(']');
                leftAsString = leftAsString.Substring(leftIndex + 4, leftAsString.Length - leftIndex - 8);
                var rightAsString = right.ToString();
                var rightIndex = rightAsString.IndexOf(']');
                rightAsString = rightAsString.Substring(rightIndex + 4, rightAsString.Length - rightIndex - 8);
                switch (operatorText)
                {
                    case "+":
                        var str = leftAsString + rightAsString;
                        var bytes = Encoding.Default.GetBytes(str);

                        fixed (byte* p = bytes)
                        {
                            sbyte* sp = (sbyte*)p;
                            valueStack.Push(LLVM.ConstString(sp, (uint)str.Length, 0));
                        }

                        break;
                    default:
                        throw new InvalidOperationException("Unsupported operation");
                }

                break;
        }

        logger.Log($"left={left}, right={right}, leftType={left.TypeOf.Kind}, rightType={right.TypeOf.Kind}");
        return context;
    }

    private static bool CheckType(string type, LLVMTypeKind valueType)
    {
        return Enum.TryParse(type, true, out VariableType variableType) && variableType switch
        {
            VariableType.Int => valueType is LLVMTypeKind.LLVMIntegerTypeKind,
            VariableType.String => valueType is LLVMTypeKind.LLVMArrayTypeKind,
            _ => false,
        };
    }
}