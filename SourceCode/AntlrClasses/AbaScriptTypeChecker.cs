using AbaScript.AntlrClasses.Exceptions;
using AbaScript.AntlrClasses.Models;

namespace AbaScript.AntlrClasses;

public class AbaScriptTypeChecker : AbaScriptBaseVisitor<AbaType>
{
    private readonly Dictionary<string, Dictionary<string, AbaType>> _classFields = new();
    private readonly Dictionary<string, List<AbaType>> _functionParamTypes = new();
    private readonly Dictionary<string, AbaType> _functionReturnTypes = new();
    private Scope _currentScope = new();

    public override AbaType VisitFunctionDef(AbaScriptParser.FunctionDefContext context)
    {
        var functionName = context.ID().GetText();
        var returnType = Visit(context.returnType());
        _functionReturnTypes[functionName] = returnType;

        _currentScope = new Scope(_currentScope);

        var paramTypes = new List<AbaType>();
        foreach (var param in context.typedParam())
        {
            var paramType = Visit(param.type());
            var paramName = param.ID().GetText();
            _currentScope.variables[paramName] = paramType;
            paramTypes.Add(paramType);
        }

        _functionParamTypes[functionName] = paramTypes;

        Visit(context.block());
        _currentScope = _currentScope.parent ?? throw new InvalidOperationException("Parent scope cannot be null.");

        return returnType;
    }

    public override AbaType VisitFuncCall(AbaScriptParser.FuncCallContext context)
    {
        var functionName = context.ID().GetText();

        if (!_functionReturnTypes.ContainsKey(functionName))
            throw new TypeCheckerException($"Function '{functionName}' not defined.");

        var expectedParamTypes = _functionParamTypes[functionName];
        var actualParamTypes = context.expr().Select(Visit).ToList();

        if (expectedParamTypes.Count != actualParamTypes.Count)
            throw new TypeCheckerException(
                $"Incorrect number of arguments for function '{functionName}'. Expected {expectedParamTypes.Count}, got {actualParamTypes.Count}.");

        for (var i = 0; i < expectedParamTypes.Count; i++)
            if (!Equals(expectedParamTypes[i], actualParamTypes[i]))
                throw new TypeCheckerException(
                    $"Type mismatch for argument {i + 1} of function '{functionName}'. Expected {expectedParamTypes[i]}, got {actualParamTypes[i]}.");

        return _functionReturnTypes[functionName];
    }

    public override AbaType VisitClassDef(AbaScriptParser.ClassDefContext context)
    {
        var className = context.ID().GetText();
        _classFields.Add(className, new Dictionary<string, AbaType>());

        _currentScope = new Scope(_currentScope);
        foreach (var member in context.classMember())
            if (member.variableDeclaration() != null)
            {
                var varDecl = member.variableDeclaration();
                var fieldType = Visit(varDecl.type());
                var fieldName = varDecl.ID().GetText();
                _classFields[className].Add(fieldName, fieldType);
                _currentScope.variables[fieldName] = fieldType;
            }
            else if (member.functionDef() != null)
            {
                Visit(member.functionDef());
            }

        _currentScope = _currentScope.parent ?? throw new InvalidOperationException();

        return new AbaType(className);
    }

    public override AbaType VisitFieldAccess(AbaScriptParser.FieldAccessContext context)
    {
        var varName = context.ID(0).GetText();
        var fieldName = context.ID(1).GetText();

        var classType = _currentScope.Resolve(varName);

        if (Equals(classType, AbaType.Error)) throw new TypeCheckerException($"Variable '{varName}' not declared.");
        if (!_classFields.ContainsKey(classType.ToString()))
            throw new TypeCheckerException($"Variable '{varName}' is not a class.");

        if (!_classFields[classType.ToString()].ContainsKey(fieldName))
            throw new TypeCheckerException($"Class '{classType}' does not contain field '{fieldName}'.");

        return _classFields[classType.ToString()][fieldName];
    }

    public override AbaType VisitVariableDeclaration(AbaScriptParser.VariableDeclarationContext context)
    {
        var type = Visit(context.type());
        if (context.NUMBER() != null)
        {
            type.IsArray = true;
        }

        var varName = context.ID().GetText();

        if (_currentScope.variables.ContainsKey(varName))
            throw new TypeCheckerException($"Variable '{varName}' already declared.");

        _currentScope.variables.Add(varName, type);

        if (context.expr() != null)
        {
            var exprType = Visit(context.expr());
            if (!Equals(exprType, type))
                throw new TypeCheckerException(
                    $"Type mismatch in variable declaration. Expected '{type}', but got '{exprType}'.");
        }

        return type;
    }

    public override AbaType VisitAssignment(AbaScriptParser.AssignmentContext context)
    {
        AbaType leftType;
        if (context.ID() != null)
        {
            var varName = context.ID().GetText();
            leftType = _currentScope.Resolve(varName);
            if (Equals(leftType, AbaType.Error)) throw new TypeCheckerException($"Variable '{varName}' not declared.");
        }
        else
        {
            leftType = Visit(context.fieldAccess());
        }

        AbaType rightType;
        if (context.expr(1) != null)
            rightType = Visit(context.expr(1));
        else
            rightType = Visit(context.expr(0));

        if (!Equals(leftType, rightType))
            throw new TypeCheckerException(
                $"Type mismatch in assignment. Cannot assign '{rightType}' to '{leftType}'.");

        return leftType;
    }

    public override AbaType VisitType(AbaScriptParser.TypeContext context)
    {
        if (context.GetText() == "int") return AbaType.Int;
        if (context.GetText() == "string") return AbaType.String;
        if (context.GetText() == "bool") return AbaType.Bool;
        if (context.ID() != null) return new AbaType(context.ID().GetText());
        return AbaType.Error;
    }

    public override AbaType VisitAddSub(AbaScriptParser.AddSubContext context)
    {
        var leftType = Visit(context.expr());
        var rightType = Visit(context.term());

        if (!Equals(leftType, AbaType.Int) || !Equals(rightType, AbaType.Int))
            throw new TypeCheckerException(
                $"Type mismatch: both operands of '+' or '-' must be integers. Got {leftType} and {rightType}");

        return AbaType.Int;
    }

    public override AbaType VisitMulDivMod(AbaScriptParser.MulDivModContext context)
    {
        var leftType = Visit(context.term());
        var rightType = Visit(context.factor());

        if (!Equals(leftType, AbaType.Int) || !Equals(rightType, AbaType.Int))
            throw new TypeCheckerException(
                $"Type mismatch: both operands of '*', '/', or '%' must be integers. Got {leftType} and {rightType}");

        return AbaType.Int;
    }

    public override AbaType VisitVariableOrArrayAccess(AbaScriptParser.VariableOrArrayAccessContext context)
    {
        if (context.expr() == null)
        {
            var identifier = context.ID().GetText();
            var type = _currentScope.Resolve(identifier);
            if (Equals(type, AbaType.Error))
            {
                throw new TypeCheckerException($"Variable '{identifier}' not defined.");
            }

            return type;
        }
        else
        {
            var identifier = context.ID().GetText();
            var type = _currentScope.Resolve(identifier);

            if (Equals(type, AbaType.Error))
            {
                throw new TypeCheckerException($"Variable '{identifier}' not defined.");
            }

            if (!type.IsArray)
            {
                throw new TypeCheckerException($"Variable '{identifier}' is not an array.");
            }

            var indexType = Visit(context.expr());
            if (!Equals(indexType, AbaType.Int))
            {
                throw new TypeCheckerException($"Array index must be an integer. Got {indexType}.");
            }

            return new AbaType(type.Name);
        }
    }

    public override AbaType VisitReturnType(AbaScriptParser.ReturnTypeContext context)
    {
        return Visit(context.type());
    }
}