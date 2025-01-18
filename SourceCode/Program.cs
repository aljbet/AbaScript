using System.Reflection;
using AbaScript.Services;

// var curFileName = "3-benchmarks\\array-sorting.as";
// var curFileName = "3-benchmarks\\factorial.as";
var curFileName = "3-benchmarks\\prime-numbers.as";
// var curFileName = "functions.as";
var path = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location)
           + "\\..\\..\\..\\..\\abas_cripts\\" + curFileName;
var input = File.ReadAllText(path);

Console.WriteLine(curFileName);
var runService = new InterpreterService();
// var runService = new LlvmService();
runService.RunCode(input);