using System.Reflection;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Kronstadt.Core.Commands.Framework;
using Kronstadt.Core.Logging;

namespace Kronstadt.Core.Plugins;

internal class PluginManager
{
    private static readonly HashSet<IPlugin> _Plugins = new();

    private static readonly ILogger _Logger;
    static PluginManager()
    {
        _Logger = LoggerProvider.CreateLogger<PluginManager>();
    }

    private const string PluginsDirectory = "Plugins";
    public static async UniTask LoadPlugins()
    {
        Directory.CreateDirectory(PluginsDirectory);
        string[] paths = Directory.GetFiles(PluginsDirectory, "*.dll");
        foreach (string path in paths)
        {
            Assembly assembly = Assembly.LoadFile(Path.GetFullPath(path));
            await LoadPlugin(assembly);
        }
    }

    public static async UniTask UnloadPlugins()
    {
        foreach (IPlugin plugin in _Plugins)
        {
            try
            {
                _Logger.LogInformation($"Unloading {plugin.GetType().Assembly.FullName}");
                await plugin.UnloadAsync();
            }
            catch (Exception exception)
            {
                _Logger.LogError(exception, $"Failed to unload {plugin.GetType().Assembly.FullName}");
            }
        }
    }

    private static async UniTask WriteAllManifestResources(string working, Assembly assembly, string[] resources)
    {
        foreach (string resourse in resources)
        {
            string path = Path.Combine(working, resourse);
            if (File.Exists(path))
            {
                continue;
            }

            _Logger.LogDebug($"Writing manifest resource {resourse}");
            using StreamReader reader = new(assembly.GetManifestResourceStream(resourse));
            string text = await reader.ReadToEndAsync();

            using StreamWriter writer = new(path);
            await writer.WriteAsync(text);
        }
    }

    private static async UniTask LoadPlugin(Assembly assembly)
    {
        Type? pluginType = assembly.GetTypes().FirstOrDefault(x => x.GetInterfaces().Contains(typeof(IPlugin)));
        if (pluginType == null)
        {
            return;
        }

        IPlugin plugin = (IPlugin)Activator.CreateInstance(pluginType);
        plugin.SetWorkingDirectory(Directory.GetCurrentDirectory() + "/Plugins/" + assembly.GetName().Name);
        Directory.CreateDirectory(plugin.WorkingDirectory);

        await WriteAllManifestResources(plugin.WorkingDirectory, assembly, assembly.GetManifestResourceNames());

        _Logger.LogInformation($"Loading plugin {assembly.FullName}");
        bool failed = false;
        try
        {
            await plugin.LoadAsync();
            CommandManager.RegisterCommandTypes(assembly);
        }
        catch (Exception exception)
        {
            failed = true;
            _Logger.LogError(exception, $"Failed to load plugin {assembly.FullName}");
        }

        if (!failed)
        {
            _Plugins.Add(plugin);
        }
    }
}
