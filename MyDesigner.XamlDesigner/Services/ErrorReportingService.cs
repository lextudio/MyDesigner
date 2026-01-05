using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace MyDesigner.XamlDesigner.Services
{
    /// <summary>
    /// Service for handling error reporting and logging
    /// </summary>
    public static class ErrorReportingService
    {
        private static readonly string LogPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "MyDesigner.XamlDesigner",
            "logs",
            "error.log");

        public static void ReportException(Exception exception)
        {
            if (exception == null) return;

            try
            {
                // Log to debug output
                Debug.WriteLine($"Exception: {exception}");

                // Log to file
                LogToFile(exception);

                // In a real application, you might want to show a user-friendly error dialog
                // For now, we'll just log it
            }
            catch (Exception logException)
            {
                // If logging fails, at least output to debug
                Debug.WriteLine($"Failed to log exception: {logException}");
                Debug.WriteLine($"Original exception: {exception}");
            }
        }

        private static void LogToFile(Exception exception)
        {
            try
            {
                var directory = Path.GetDirectoryName(LogPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var logEntry = new StringBuilder();
                logEntry.AppendLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR");
                logEntry.AppendLine($"Message: {exception.Message}");
                logEntry.AppendLine($"Type: {exception.GetType().FullName}");
                logEntry.AppendLine($"Stack Trace: {exception.StackTrace}");
                
                if (exception.InnerException != null)
                {
                    logEntry.AppendLine($"Inner Exception: {exception.InnerException.Message}");
                    logEntry.AppendLine($"Inner Stack Trace: {exception.InnerException.StackTrace}");
                }
                
                logEntry.AppendLine(new string('-', 80));

                File.AppendAllText(LogPath, logEntry.ToString());
            }
            catch
            {
                // Ignore logging errors to prevent infinite loops
            }
        }

        public static void ReportWarning(string message)
        {
            try
            {
                Debug.WriteLine($"Warning: {message}");
                
                var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] WARNING: {message}\n";
                
                var directory = Path.GetDirectoryName(LogPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                File.AppendAllText(LogPath, logEntry);
            }
            catch
            {
                // Ignore logging errors
            }
        }

        public static void ReportInfo(string message)
        {
            try
            {
                Debug.WriteLine($"Info: {message}");
                
                var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] INFO: {message}\n";
                
                var directory = Path.GetDirectoryName(LogPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                File.AppendAllText(LogPath, logEntry);
            }
            catch
            {
                // Ignore logging errors
            }
        }
    }
}