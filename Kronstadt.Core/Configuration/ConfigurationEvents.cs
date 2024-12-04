namespace Kronstadt.Core.Configuration;

public delegate void ConfigurationReloaded();

public static class ConfigurationEvents
{
    public static ConfigurationReloaded? OnConfigurationReloaded;
}
