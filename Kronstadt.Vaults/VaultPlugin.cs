using Cysharp.Threading.Tasks;
using Kronstadt.Core.Extensions;
using Kronstadt.Core.Logging;
using Kronstadt.Core.Players;
using Kronstadt.Core.Plugins;
using Kronstadt.Core.Ranks;
using Kronstadt.Vaults.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Steamworks;

namespace Kronstadt.Vaults;

internal class VaultPlugin : Plugin
{
    private string[] Ranks = 
    [
        "default",
        "vip",
        "vipplus",
        "mvp",
        "mvpplus",
        "pro",
        "proplus"
    ];

    public readonly static Dictionary<string, VaultData> VaultDatas = new();
    public readonly static Dictionary<CSteamID, IEnumerable<VaultItems>> Vaults = new();
    private readonly ILogger _Logger;
    private string _DataDirectory = null!;
    public VaultPlugin()
    {
        _Logger = LoggerProvider.CreateLogger<VaultPlugin>();
    }

    public override async UniTask LoadAsync()
    {
        _DataDirectory = Path.Combine(WorkingDirectory, "Data");
        Directory.CreateDirectory(_DataDirectory);

        string content;
        using (StreamReader reader = new(Path.Combine(WorkingDirectory, "Kronstadt.Vaults.Configuration.json")))
        {
            content = await reader.ReadToEndAsync();
        }

        IEnumerable<VaultData> vaults = JsonConvert.DeserializeObject<IEnumerable<VaultData>>(content) ?? throw new("Failed to get vault data");
        foreach (VaultData vault in vaults)
        {
            VaultDatas.Add(vault.Name, vault);
        }

        KronstadtPlayerManager.OnPlayerConnected += OnPlayerConnected;
        KronstadtPlayerManager.OnPlayerDisconnected += OnPlayerDisconnected;
    }

    public override UniTask UnloadAsync()
    {
        KronstadtPlayerManager.OnPlayerConnected -= OnPlayerConnected;
        KronstadtPlayerManager.OnPlayerDisconnected -= OnPlayerDisconnected;
        return UniTask.CompletedTask;
    }
    
    private async UniTask AddVaults(KronstadtPlayer player, Rank rank)
    {
        Dictionary<string, Vault> vaults = (await ReadPlayerVaultData(player)).ToDictionary(x => x.Name);
        byte max = (byte)rank;
        max++;
        for (int i = 0; i < max; i++)
        {
            vaults.TryAdd(Ranks[i], new(Ranks[i]));
        }

        await WritePlayerVaultData(player, vaults.Select(x => x.Value).ToList());
    }

    private async UniTask BindVaults(KronstadtPlayer player)
    {
        List<Vault> vaults = await ReadPlayerVaultData(player);
        List<VaultItems> items = new(vaults.Count());
        foreach (Vault vault in vaults)
        {
            VaultData data = VaultDatas[vault.Name];
            items.Add(new(vault, data));
        }

        Vaults.Add(player.SteamID, items);
    }

    private async void OnPlayerConnected(KronstadtPlayer player)
    {
        Rank rank = await RankManager.GetRankAsync(player.SteamID);

        try
        {
            await AddVaults(player, rank);
            await BindVaults(player);
        }
        catch (Exception ex)
        {
            _Logger.LogError(ex, "Failed to bind vaults");
        }
    }

    private void OnPlayerDisconnected(KronstadtPlayer player)
    {
        if (!Vaults.TryGetValue(player.SteamID, out IEnumerable<VaultItems> items))
        {
            return;
        }
        Vaults.Remove(player.SteamID);
        
        VaultItems[] vaultItems = items.ToArray();
        List<Vault> vaults = new(vaultItems.Length);
        foreach (VaultItems vaultItem in vaultItems)
        {
            vaults.Add(new(vaultItem));
        }

        WritePlayerVaultData(player, vaults).Forget();
    }

    private async UniTask WritePlayerVaultData(KronstadtPlayer player, List<Vault> vaults)
    {
        string content = JsonConvert.SerializeObject(vaults);
        using StreamWriter writer = new(File.Open(Path.Combine(_DataDirectory, $"{player.SteamID}.json"), FileMode.Create, FileAccess.Write));
        await writer.WriteAsync(content);
    }

    private async UniTask<List<Vault>> ReadPlayerVaultData(KronstadtPlayer player)
    {
        string path = Path.Combine(_DataDirectory, $"{player.SteamID}.json");
        if (!File.Exists(path))
        {
            return new();
        }

        using StreamReader reader = new(File.Open(path, FileMode.Open, FileAccess.Read));
        string data = await reader.ReadToEndAsync();

        return JsonConvert.DeserializeObject<List<Vault>>(data) ?? throw new("Failed to read vault data");
    }
}
