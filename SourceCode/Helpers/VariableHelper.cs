﻿using AbaScript.AntlrClasses.Models;

namespace AbaScript.Helpers;

public static class VariableHelper
{
    public static object? ConvertValue(VariableType type, object value)
    {
        return type switch
        {
            VariableType.Int => Convert.ToInt32(value),
            VariableType.String => value.ToString(),
            VariableType.Array => value as object[],
            _ => throw new InvalidOperationException("Unsupported variable type")
        };
    }
}