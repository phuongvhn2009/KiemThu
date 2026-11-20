using NUnit.Framework;

namespace SauceDemo.Tests.Utilities;

public static class Log
{
    private static readonly object FileLock = new();

    public static string Info(string message, string? logFilePath = null)
        => Write("INFO", message, logFilePath);

    public static string Error(string message, string? logFilePath = null)
        => Write("ERROR", message, logFilePath);

    private static string Write(string level, string message, string? logFilePath)
    {
        var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{level}] [T{Environment.CurrentManagedThreadId:00}] {message}";
        TestContext.Progress.WriteLine(line);
        if (!string.IsNullOrWhiteSpace(logFilePath))
        {
            lock (FileLock)
            {
                var dir = Path.GetDirectoryName(logFilePath);
                if (!string.IsNullOrWhiteSpace(dir)) Directory.CreateDirectory(dir);
                File.AppendAllText(logFilePath, line + Environment.NewLine);
            }
        }
        return line;
    }
}
