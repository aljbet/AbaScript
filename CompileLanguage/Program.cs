using System.Reflection;
using CompileLanguage.Services;

var curFileName = "example.as";
var path = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location)
           + "\\..\\..\\..\\..\\SourceCode\\abas_cripts\\" + curFileName;

var input = File.ReadAllText(path);

Console.WriteLine(curFileName);
var runService = new CompilerService();
runService.Execute(input);