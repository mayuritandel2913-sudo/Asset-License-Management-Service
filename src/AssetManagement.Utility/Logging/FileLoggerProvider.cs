using Microsoft.Extensions.Logging;

namespace AssetManagement.Utility.Logging;

public class FileLoggerProvider : ILoggerProvider
{
    private readonly string _filePath;
    private bool _disposed;

    public FileLoggerProvider(string filePath)
    {
        _filePath = filePath;
        var dir = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);
    }

    public ILogger CreateLogger(string categoryName)
        => new FileLogger(categoryName, _filePath);

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // free managed resources here if needed
            }

            // free unmanaged resources here if needed
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}