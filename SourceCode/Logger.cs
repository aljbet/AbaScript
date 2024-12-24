using System.Reflection;

namespace AbaScript;

public class Logger
{
    private readonly string logFilePath;

    public Logger()
    {
        logFilePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\..\\..\\..\\..\\logs\\" +
                      $"log_{DateTime.Now:yyyyMMdd_HHmmss}.log";
    }

    public void Log(string message)
    {
        using (var writer = new StreamWriter(logFilePath, true))
        {
            writer.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}");
        }
    }
}