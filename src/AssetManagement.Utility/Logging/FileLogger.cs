using Microsoft.Extensions.Logging;
namespace AssetManagement.Utility.Logging;
public class FileLogger : ILogger
{
    private readonly string _categoryName;
    private readonly string _filePath;
    private static readonly object _lock = new();

    public FileLogger(string categoryName, string filePath)
    {
        _categoryName = categoryName;
        _filePath = filePath;
    }
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;
    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;

        var message = formatter(state, exception);
        var logEntry = $"[{DateTime.Now:MM-dd-yyyy HH:mm:ss}] [{logLevel}] [{_categoryName}] {message}";

        if (exception != null)
            logEntry += Environment.NewLine + exception.ToString();

        lock (_lock)
        {
            File.AppendAllText(_filePath, logEntry + Environment.NewLine);
        }
    }
}