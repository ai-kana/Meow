using Cysharp.Threading.Tasks;
using Meow.Core.Json;
using Newtonsoft.Json;
using Steamworks;

namespace Meow.Core.Players;

internal static class PlayerDataManager
{
    private const string DataDirectory = "PlayerData";

    static PlayerDataManager()
    {
        Directory.CreateDirectory(DataDirectory);
        ServerManager.OnServerSave += OnSave;
    }

    private static void OnSave()
    {
        foreach (MeowPlayer player in MeowPlayerManager.Players)
        {
            _ = SaveDataAsync(player);
        }
    }

    public static async UniTask<PlayerData> LoadDataAsync(CSteamID steamID)
    {
        string path = $"{DataDirectory}/{steamID}.json";

        if (!File.Exists(path))
        {
            return new();
        }

        using JsonStreamReader reader = new(File.Open(path, FileMode.Open, FileAccess.Read));
        return await reader.ReadObject<PlayerData>() ?? new();
    }

    public static async UniTask SaveDataAsync(MeowPlayer player)
    {
        string path = $"{DataDirectory}/{player.SteamID}.json";

        using JsonStreamWriter writer = new(File.Open(path, FileMode.Create, FileAccess.Write));
        await writer.WriteObject(player.SaveData);
    }
}
