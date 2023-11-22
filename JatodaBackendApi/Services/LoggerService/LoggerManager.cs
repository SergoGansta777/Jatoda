using NLog;
using JatodaBackendApi.Services.LoggerService.Interfaces;
using ILogger = NLog.ILogger;

namespace JatodaBackendApi.Services.LoggerService;

public class LoggerManager : ILoggerManager
{
    private static ILogger _logger = LogManager.GetCurrentClassLogger();

    public LoggerManager()
    {}

    public void LogDebug(string message) => _logger.Debug(message);
    public void LogError(string message) => _logger.Error(message);
    public void LogInfo(string message) => _logger.Info(message);
    public void LogWarn(string message) => _logger.Warn(message);
}