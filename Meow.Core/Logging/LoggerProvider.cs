using Meow.Core.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Meow.Core.Logging;

public static class LoggerProvider
{
    private static ILoggerProvider? _Provider;
    public static LogLevel AllowedLevel {get; private set;}

    static LoggerProvider()
    {
        ServerManager.OnPreShutdown += OnPreShutdown;
        ConfigurationEvents.OnConfigurationReloaded += OnReloaded;
    }

    public static bool AddLogging(ILoggerProvider provider)
    {
        if (_Provider != null)
        {
            return false;
        }

        _Provider = provider;
        return true;
    }

    public static ILogger CreateLogger<T>()
    {
        return _Provider?.CreateLogger<T>() ?? throw new();
    }

    public static ILogger CreateLogger(string name)
    {
        return _Provider?.CreateLogger(name) ?? throw new();
    }

    private static void OnReloaded()
    {
        string level = MeowHost.Configuration.GetValue<string>("LoggingLevel") ?? "None";
        AllowedLevel = (LogLevel)Enum.Parse(typeof(LogLevel), level);
    }

    private static void OnPreShutdown()
    {
        _Provider?.Dispose();
    }
}
