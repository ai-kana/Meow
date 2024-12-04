using Microsoft.Extensions.Logging;

namespace Kronstadt.Core.Logging;

// Possibly a questionable design choice but I see this as a more functional way to use this concept
public static class LoggerProvider
{
    private static ILoggerProvider? _Provider;

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

    static LoggerProvider()
    {
        ServerManager.OnPreShutdown += OnPreShutdown;
    }

    private static void OnPreShutdown()
    {
        _Provider?.Dispose();
    }
}
