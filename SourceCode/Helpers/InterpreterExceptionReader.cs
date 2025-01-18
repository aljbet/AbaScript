using Antlr4.Runtime;

namespace AbaScript.Helpers;

public class InterpreterExceptionReader : ICanReadException
{
    public void HandleException(Exception ex)
    {
        switch (ex)
        {
            case RecognitionException:
                Console.WriteLine($"Syntax error: {ex.Message}");
                break;
            case InvalidOperationException invalidOperationException:
                Console.WriteLine($"Runtime error: {invalidOperationException.Message}");
                break;
            default:
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
                break;
        }
    }
}