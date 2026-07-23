using System;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;

namespace ScreenBuddy.Infrastructure.Logging
{
    /// <summary>
    /// File logger provider creating rotating log file sinks in %APPDATA%\ScreenBuddy\.
    /// </summary>
    public sealed class FileLoggerProvider : ILoggerProvider
    {
        private readonly string _logFilePath;

        public FileLoggerProvider(string? directoryPath = null)
        {
            string dir = directoryPath ?? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "ScreenBuddy");

            Directory.CreateDirectory(dir);
            _logFilePath = Path.Combine(dir, "screenbuddy.log");
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new FileLogger(categoryName, _logFilePath);
        }

        public void Dispose()
        {
        }
    }

    /// <summary>
    /// Thread-safe rotating file logger (1MB limit, 2 file rotation).
    /// </summary>
    public sealed class FileLogger : ILogger
    {
        private static readonly object LogLock = new();
        private const long MaxFileSizeBytes = 1024 * 1024; // 1 MB
        private readonly string _categoryName;
        private readonly string _filePath;

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
            if (!IsEnabled(logLevel))
            {
                return;
            }

            string message = formatter(state, exception);
            if (string.IsNullOrEmpty(message) && exception == null)
            {
                return;
            }

            var sb = new StringBuilder();
            sb.Append(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff UTC"));
            sb.Append(" [").Append(logLevel.ToString()).Append("] ");
            sb.Append("[").Append(_categoryName).Append("] ");
            sb.Append(message);

            if (exception != null)
            {
                sb.AppendLine().Append(exception.ToString());
            }

            sb.AppendLine();
            string line = sb.ToString();

            lock (LogLock)
            {
                try
                {
                    RotateLogFileIfNeeded();
                    File.AppendAllText(_filePath, line, Encoding.UTF8);
                }
                catch
                {
                    // Fail-safe: logging failure must never crash the application
                }
            }
        }

        private void RotateLogFileIfNeeded()
        {
            if (!File.Exists(_filePath))
            {
                return;
            }

            var fi = new FileInfo(_filePath);
            if (fi.Length >= MaxFileSizeBytes)
            {
                string backupPath = Path.Combine(fi.DirectoryName!, "screenbuddy.1.log");
                try
                {
                    if (File.Exists(backupPath))
                    {
                        File.Delete(backupPath);
                    }
                    File.Move(_filePath, backupPath);
                }
                catch
                {
                    // Ignore rotation failure
                }
            }
        }
    }
}
