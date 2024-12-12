using Cysharp.Threading.Tasks;
using Meow.Core.Extensions;
using Meow.Core.Json;
using Meow.Core.Logging;
using Meow.Core.Players;
using Meow.Core.Plugins;
using Meow.Core.Ranks;
using Meow.Vaults.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Steamworks;

namespace Meow.Vaults;

internal class VaultPlugin : Plugin
{
    private string[] Ranks = 
    [
        "default",
        "vip",
        "vipplus",
        "mvp",
        "mvpplus",
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

        IEnumerable<VaultData> vaults;
        using (JsonStreamReader reader = new(File.Open(Path.Combine(WorkingDirectory, "Meow.Vaults.Configuration.json"), FileMode.Open, FileAccess.Read)))
        {
            vaults = await reader.ReadObject<IEnumerable<VaultData>>();
        }

        foreach (VaultData vault in vaults)
        {
            VaultDatas.Add(vault.Name, vault);
        }

        MeowPlayerManager.OnPlayerConnected += OnPlayerConnected;
        MeowPlayerManager.OnPlayerDisconnected += OnPlayerDisconnected;
    }

    public override UniTask UnloadAsync()
    {
        MeowPlayerManager.OnPlayerConnected -= OnPlayerConnected;
        MeowPlayerManager.OnPlayerDisconnected -= OnPlayerDisconnected;
        return UniTask.CompletedTask;
    }
    
    private async UniTask AddVaults(MeowPlayer player, Rank rank)
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

    private async UniTask BindVaults(MeowPlayer player)
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

    private async void OnPlayerConnected(MeowPlayer player)
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

    private void OnPlayerDisconnected(MeowPlayer player)
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

    private async UniTask WritePlayerVaultData(MeowPlayer player, List<Vault> vaults)
    {
        using JsonStreamWriter writer = new(File.Open(Path.Combine(_DataDirectory, $"{player.SteamID}.json"), FileMode.Create, FileAccess.Write));
        await writer.WriteObject(vaults);
    }

    private async UniTask<List<Vault>> ReadPlayerVaultData(MeowPlayer player)
    {
        string path = Path.Combine(_DataDirectory, $"{player.SteamID}.json");
        if (!File.Exists(path))
        {
            return new();
        }

        using JsonStreamReader reader = new(File.Open(path, FileMode.Open, FileAccess.Read));
        return await reader.ReadObject<List<Vault>>();
    }
}
