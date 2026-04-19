namespace CitySO.Services
{
    public interface ILogger
    {
        void LogInfo(string message);
        void LogWarning(string message);
        void LogError(string message);
        IEnumerable<string> GetLogs();
    }
}