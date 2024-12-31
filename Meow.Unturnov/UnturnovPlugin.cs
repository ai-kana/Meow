using Cysharp.Threading.Tasks;
using Meow.Core.Json;
using Meow.Core.Players;
using Meow.Core.Plugins;
using Microsoft.Extensions.Configuration;
using Meow.Core.Extensions;
using Steamworks;
using UnityEngine;

namespace Meow.Unturnov;

public class UnturnovPlugin : Plugin
{
    public static IConfiguration Configuration = null!;

    public static Dictionary<CSteamID, Vector3> DoorPositions {get; private set;} = new();

    public string DataDirectory {get; private set;} = string.Empty;
    public string PlayerDataPath(MeowPlayer player) => Path.Combine(DataDirectory, $"{player.SteamID}.json");
    public override UniTask LoadAsync()
    {
        ConfigurationBuilder builder = new();
        builder.AddJsonFile(Path.Combine(WorkingDirectory, "Meow.Unturnov.Configuration.json"));
        Configuration = builder.Build();

        DataDirectory = Path.Combine(WorkingDirectory, "Data");

        MeowPlayerManager.OnPlayerConnected += OnPlayerConnected;
        MeowPlayerManager.OnPlayerDisconnected += OnPlayerDisconnected;

        return UniTask.CompletedTask;
    }

    public override UniTask UnloadAsync()
    {
        MeowPlayerManager.OnPlayerConnected -= OnPlayerConnected;
        MeowPlayerManager.OnPlayerDisconnected -= OnPlayerDisconnected;
        return UniTask.CompletedTask;
    }

    private async void OnPlayerConnected(MeowPlayer player)
    {
        string path = PlayerDataPath(player);
        if (!File.Exists(path))
        {
            return;
        }

        using JsonStreamReader reader = new(File.Open(path, FileMode.Open, FileAccess.Read));
        Position position = await reader.ReadObject<Position>();
        DoorPositions.AddOrUpdate(player.SteamID, position.ToVector3());
    }

    private async void OnPlayerDisconnected(MeowPlayer player)
    {
        if (!DoorPositions.TryGetValue(player.SteamID, out Vector3 value))
        {
            return;
        }

        string path = PlayerDataPath(player);
        using JsonStreamWriter writer = new(File.Open(path, FileMode.Create, FileAccess.Write));
        await writer.WriteObject(Position.FromVector3(value));
        DoorPositions.Remove(player.SteamID);
    }
}
