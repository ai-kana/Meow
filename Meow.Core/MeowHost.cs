using Meow.Core.Commands.Framework; 
using Meow.Core.Logging; 
using Meow.Core.Roles;
using Meow.Core.Players;
using Meow.Core.Translations;
using Meow.Core.Offenses;
using Meow.Core.Bot;
using Meow.Core.Ranks;
using Meow.Core.Plugins;
using Meow.Core.Stats;
using Meow.Core.Zones;
using System.Reflection;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UnityEngine.LowLevel;
using SDG.Unturned;
using HarmonyLib;
using Meow.Core.Manifest;

namespace Meow.Core;

public sealed class MeowHost 
{
    private ILogger? _Logger;
    private Harmony? _Harmony;

    public static IConfiguration Configuration {get; private set;} = null!;

    private async UniTask<IConfiguration> CreateConfigurationAsync()
    {
        const string manifestPath = "Meow.Core.Configuration.json";
        const string configPath = "Configuration.json";

        if (!File.Exists("Configuration.json"))
        {
            await ManifestHelper.CopyToFile(manifestPath, configPath);
        }

        ConfigurationBuilder configurationBuilder = new();
        configurationBuilder.SetBasePath(Directory.GetCurrentDirectory());
        configurationBuilder.AddJsonFile(configPath);
        return configurationBuilder.Build();
    }

    public async UniTask LoadAsync()
    {
        Directory.CreateDirectory(AppContext.BaseDirectory + "/Meow");
        Directory.SetCurrentDirectory(AppContext.BaseDirectory + "/Meow");
        Configuration = await CreateConfigurationAsync();

        await TranslationManager.LoadTranslations();

        LoggerProvider.AddLogging(new MeowLoggerProvider($"./Logs/Log.log"));
        _Logger = LoggerProvider.CreateLogger<MeowHost>()!;
        _Logger.LogInformation("Starting Unturnov...");

        ThreadConsole console = new();
        Dedicator.commandWindow?.removeDefaultIOHandler();
        Dedicator.commandWindow?.addIOHandler(console);

        _Harmony = new("Meow.Core");
        _Harmony.PatchAll();

        MeowPlayerManager.Load();

        CommandManager.RegisterCommandTypes(Assembly.GetExecutingAssembly());
        await RoleManager.RegisterRoles();

        await OffenseManager.CreateTables();
        await PlayerIdManager.CreateTables();
        await LinkManager.CreateTables();
        await RankManager.CreateTables();
        await StatsManager.CreateTables();

        await ZoneManager.LoadZonesAsync();

        _ = BotManager.Start();

        await PluginManager.LoadPluginsAsync();

        _Logger.LogInformation("Started meow!");
    }

    public UniTask UnloadAsync()
    { 
        _Harmony?.UnpatchAll();
        return UniTask.CompletedTask;
    }
}
