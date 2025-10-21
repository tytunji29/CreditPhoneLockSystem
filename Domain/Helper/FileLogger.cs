using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

public static class FileLogger
{
    private static readonly string LogDirectory = Path.Combine(AppContext.BaseDirectory, "Logs");
    private static readonly string LogFilePath = Path.Combine(LogDirectory, $"log_{DateTime.Now:yyyyMMdd}.txt");

    public static async Task WriteLogAsync(string message)
    {
        try
        {
            // Ensure Logs directory exists
            if (!Directory.Exists(LogDirectory))
                Directory.CreateDirectory(LogDirectory);

            var logMessage = new StringBuilder();
            logMessage.AppendLine("======================================");
            logMessage.AppendLine($"Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            logMessage.AppendLine($"Message: {message}");
            logMessage.AppendLine();

            await File.AppendAllTextAsync(LogFilePath, logMessage.ToString());
        }
        catch (Exception ex)
        {
            // Optionally fallback to console if file write fails
            Console.WriteLine($"Logging failed: {ex.Message}");
        }
    }
}

public static class FileHelper
{
    public static async Task<string> ConvertToBase64Async(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return null;

        using (var memoryStream = new MemoryStream())
        {
            await file.CopyToAsync(memoryStream);
            byte[] fileBytes = memoryStream.ToArray();
            return Convert.ToBase64String(fileBytes);
        }
    }
}

    public static class LoanStatus
    {
        public const string Active = "Active";
        public const string Completed = "Completed";
        public const string Locked = "Locked";
    }

    public static class RepaymentStatus
    {
        public const string Pending = "Pending";
        public const string Paid = "Paid";
        public const string Overdue = "Overdue";
    }
