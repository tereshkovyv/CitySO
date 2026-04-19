using System.IO;
using CitySO.Services;

namespace CitySO.Logging
{
    public class LoggerService : ILogger
    {
        private readonly string _logFilePath;
        private const int MaxLogs = 100;

        public LoggerService()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var appFolder = Path.Combine(appDataPath, "CitySO");
            Directory.CreateDirectory(appFolder);
            _logFilePath = Path.Combine(appFolder, "logs.txt");
        }

        public void LogInfo(string message)
        {
            Log("INFO", message);
        }

        public void LogWarning(string message)
        {
            Log("WARNING", message);
        }

        public void LogError(string message)
        {
            Log("ERROR", message);
        }

        public IEnumerable<string> GetLogs()
        {
            if (!File.Exists(_logFilePath))
                return new List<string>();

            try
            {
                var lines = File.ReadAllLines(_logFilePath);
                return lines.Reverse().Take(MaxLogs).Reverse();
            }
            catch
            {
                return new List<string>();
            }
        }

        private void Log(string level, string message)
        {
            var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";
            
            try
            {
                File.AppendAllText(_logFilePath, logEntry + Environment.NewLine);
            }
            catch
            {
                // Ignore logging errors
            }

            Console.WriteLine(logEntry);
        }
    }
}