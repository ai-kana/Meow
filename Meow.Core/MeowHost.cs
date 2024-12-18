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
using System.Runtime.InteropServices;
using System.Reflection;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UnityEngine.LowLevel;
using SDG.Unturned;
using HarmonyLib;

namespace Meow.Core;

public sealed class MeowHost 
{
    private ILogger? _Logger;
    private Harmony? _Harmony;

    public static IConfiguration Configuration {get; private set;} = null!;

    private async UniTask CreateFileAsync()
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        const string path = "Meow.Core.Configuration.json";
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
        Directory.CreateDirectory(AppContext.BaseDirectory + "/Meow");
        Directory.SetCurrentDirectory(AppContext.BaseDirectory + "/Meow");
        Configuration = await CreateConfigurationAsync();

        // Must load first or else it all breaks :)
        await TranslationManager.LoadTranslations();

        LoggerProvider.AddLogging(new MeowLoggerProvider($"./Logs/Log.log"));
        _Logger = LoggerProvider.CreateLogger<MeowHost>()!;
        _Logger.LogInformation("Starting Unturnov...");

        PlayerLoopSystem system = PlayerLoop.GetCurrentPlayerLoop();
        PlayerLoopHelper.Initialize(ref system);

        // Windows no liekly console
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            ThreadConsole console = new();
            Dedicator.commandWindow?.removeDefaultIOHandler();
            Dedicator.commandWindow?.addIOHandler(console);
        }

        MeowPlayerManager.Load();

        _Harmony = new("Meow.Core");
        _Harmony.PatchAll();

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
