namespace AbaScript;

public class Logger
{
    private readonly string _logFilePath;

    public Logger()
    {
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var directory = Path.Combine(baseDirectory, "logs");
        Directory.CreateDirectory(directory);

        _logFilePath = Path.Combine(directory, $"log_{DateTime.Now:yyyyMMdd_HHmmss}.log");
    }

    public void Log(string message)
    {
        using var writer = new StreamWriter(_logFilePath, true);
        writer.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}");
    }
}