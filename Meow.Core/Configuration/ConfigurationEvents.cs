using Meow.Core.Logging;
using Microsoft.Extensions.Logging;

namespace Meow.Core.Configuration;

public delegate void ConfigurationReloaded();

public class ConfigurationEvents
{
    public static ConfigurationReloaded? OnConfigurationReloaded;

    public static void ReloadConfiguration()
    {
        if (OnConfigurationReloaded == null)
        {
            return;
        }

        ILogger logger = LoggerProvider.CreateLogger<ConfigurationEvents>();
        foreach (ConfigurationReloaded reloadEvent in OnConfigurationReloaded.GetInvocationList().Cast<ConfigurationReloaded>())
        {
            try
            {
                reloadEvent.Invoke();
            }
            catch (Exception exception) 
            {
                logger.LogError(exception, "Exception while reloading configuration");
            }
        }
    }
}
