namespace AbaScript;

public class Logger
{
    private readonly string logFilePath;

    public Logger()
    {
        logFilePath = $"log_{DateTime.Now:yyyyMMdd_HHmmss}.log";
    }

    public void Log(string message)
    {
        using (var writer = new StreamWriter(logFilePath, true))
        {
            writer.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}");
        }
    }
}