using Microsoft.Extensions.Logging;

namespace Meow.Core.Logging;

internal static class LoggingExtensions
{
    public static ILogger CreateLogger<T>(this ILoggerProvider provider)
    {
        return provider.CreateLogger(typeof(T).FullName);
    }
}
