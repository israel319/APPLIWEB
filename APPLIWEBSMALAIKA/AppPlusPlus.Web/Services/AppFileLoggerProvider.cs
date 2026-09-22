using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace AppPlusPlus.Web.Services;

/// <summary>
/// Écrit les logs sur disque (logs/app.log) — visible sur hébergement mutualisé SmarterASP.
/// </summary>
public sealed class AppFileLoggerProvider : ILoggerProvider
{
    readonly string _filePath;
    readonly ConcurrentDictionary<string, AppFileLogger> _loggers = new();
    readonly object _writeLock = new();

    public AppFileLoggerProvider(string filePath) => _filePath = filePath;

    public ILogger CreateLogger(string categoryName) =>
        _loggers.GetOrAdd(categoryName, name => new AppFileLogger(name, _filePath, _writeLock));

    public void Dispose() => _loggers.Clear();

    sealed class AppFileLogger : ILogger
    {
        readonly string _category;
        readonly string _filePath;
        readonly object _writeLock;

        public AppFileLogger(string category, string filePath, object writeLock)
        {
            _category = category;
            _filePath = filePath;
            _writeLock = writeLock;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Warning;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            var message = formatter(state, exception);
            var line = $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}Z [{logLevel}] {_category}: {message}";
            if (exception is not null)
                line += Environment.NewLine + exception;

            try
            {
                lock (_writeLock)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
                    File.AppendAllText(_filePath, line + Environment.NewLine);
                }
            }
            catch
            {
                // Ne jamais faire échouer l'app si l'écriture disque échoue.
            }
        }
    }
}
