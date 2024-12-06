using System.Reflection;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using HarmonyLib;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SDG.Unturned;
using UnityEngine;
using Kronstadt.Core.Commands.Framework;
using Kronstadt.Core.Logging;
using Kronstadt.Core.Roles;
using Kronstadt.Core.Players;
using Kronstadt.Core.Translations;
using Kronstadt.Core.Offenses;
using Kronstadt.Core.Bot;
using Kronstadt.Core.Ranks;
using Kronstadt.Core.Plugins;
using Kronstadt.Core.Stats;

namespace Kronstadt.Core;

public sealed class KronstadtHost 
{
    private ILogger? _Logger;
    private Harmony? _Harmony;

    private GameObject? _Owner;

    public static IConfiguration Configuration {get; private set;} = null!;

    private async UniTask CreateFileAsync()
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        const string path = "Kronstadt.Core.Configuration.json";
        using StreamReader reader = new(assembly.GetManifestResourceStream(path));

        string content = await reader.ReadToEndAsync();

        using StreamWriter writer = new("Configuration.json");
        await writer.WriteAsync(content);
    }

    private async UniTask<IConfiguration> CreateConfigurationAsync()
    {
        if (!File.Exists("Configuration.json"))
        {
            await CreateFileAsync();
        }

        ConfigurationBuilder configurationBuilder = new();
        configurationBuilder.SetBasePath(Directory.GetCurrentDirectory());
        configurationBuilder.AddJsonFile("Configuration.json");
        return configurationBuilder.Build();
    }

    public async UniTask LoadAsync()
    {
        Directory.CreateDirectory(AppContext.BaseDirectory + "/Kronstadt");
        Directory.SetCurrentDirectory(AppContext.BaseDirectory + "/Kronstadt");
        Configuration = await CreateConfigurationAsync();

        LoggerProvider.AddLogging(new KronstadtLoggerProvider($"./Logs/Log.log"));
        _Logger = LoggerProvider.CreateLogger<KronstadtHost>()!;
        _Logger.LogInformation("Starting Unturnov...");

        // Windows no liekly console
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            ThreadConsole console = new();
            Dedicator.commandWindow?.removeDefaultIOHandler();
            Dedicator.commandWindow?.addIOHandler(console);
        }

        KronstadtPlayerManager.Load();

        await TranslationManager.LoadTranslations();

        _Harmony = new("Kronstadt.Core");
        _Harmony.PatchAll();

        _Owner = new("Kronstadt");
        _Owner.AddComponent<CommandQueue>();

        CommandManager.RegisterCommandTypes(Assembly.GetExecutingAssembly());
        await RoleManager.RegisterRoles();

        await OffenseManager.CreateTables();
        await PlayerIdManager.CreateTables();
        await LinkManager.CreateTables();
        await RankManager.CreateTables();
        await StatsManager.CreateTables();

        _ = BotManager.Start();

        await PluginManager.LoadPlugins();
        
        _Logger.LogInformation("Started Kronstadt!");
    }

    public UniTask UnloadAsync()
    { 
        _Harmony?.UnpatchAll();
        return UniTask.CompletedTask;
    }
}
