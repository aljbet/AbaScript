namespace AbaScript.AntlrClasses;

public class ReturnException : Exception
{
    public object ReturnValue { get; }

    public ReturnException(object returnValue)
    {
        ReturnValue = returnValue;
    }
}

public class BreakException : Exception { }
public class ContinueException : Exception { }