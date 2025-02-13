﻿using Antlr4.Runtime;

namespace AbaScript.AntlrClasses;

public class AbaScriptLexerAndParserErrorListener : IAntlrErrorListener<int>, IAntlrErrorListener<IToken>
{
    public bool HasErrors { get; private set; }

    public void SyntaxError(IRecognizer recognizer, int offendingSymbol, int line, int charPositionInLine, string msg,
        RecognitionException e)
    {
        HasErrors = true;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Lexer error at line {line}, position {charPositionInLine}: {msg}");
        Console.ResetColor();
    }

    public void SyntaxError(IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine,
        string msg, RecognitionException e)
    {
        HasErrors = true;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Parser error at line {line}, position {charPositionInLine}: {msg}");
        Console.ResetColor();
    }
}