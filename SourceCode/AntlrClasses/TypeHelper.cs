namespace AbaScript.AntlrClasses;

public static class TypeHelper
{
    public static bool AreCompatible(string? declaredType, string? exprType)
    {
        if (declaredType == null || exprType == null)
            return false;

        return declaredType.Equals(exprType) 
               || declaredType.Equals("unknown") 
               || exprType.Equals("unknown");
    }

    public static bool IsClassType(string typeName)
    {
        switch (typeName)
        {
            case "int":
            case "string":
            case "bool":
            case "void":
                return false;
            default:
                return true;
        }
    }
}