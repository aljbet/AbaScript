namespace AbaScript.AntlrClasses;

public class ReturnException : Exception
{
    public ReturnException(object returnValue)
    {
        ReturnValue = returnValue;
    }

    public object ReturnValue { get; }
}

public class BreakException : Exception
{
}

public class ContinueException : Exception
{
}