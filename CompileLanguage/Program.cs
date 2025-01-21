using System.Reflection;
using CompileLanguage.Services;

// var curFileName = "3-benchmarks\\array-sorting.as";
var curFileName = "3-benchmarks\\factorial.as";
// var curFileName = "3-benchmarks\\eratosthenes.as";
var path = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location)
           + "\\..\\..\\..\\..\\SourceCode\\abas_cripts\\" + curFileName;

var input = File.ReadAllText(path);

Console.WriteLine(curFileName);
var runService = new CompilerService();
runService.Execute(input);