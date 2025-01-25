using CompileLanguage.BaseAntlrClasses;

namespace CompileLanguage.InterpreterClasses;

public record BlockInfo(Dictionary<string, int> Labels, IList<CompiledAbaScriptParser.StatementContext> Statements, IList<string> Compiled);