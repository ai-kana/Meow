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
    public VaultPlugin()
    {
        _Logger = LoggerProvider.CreateLogger<VaultPlugin>();
    }

    public override async UniTask LoadAsync()
    {
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
    
    private IEnumerable<Vault> GetVaults(KronstadtPlayer player)
    {
        IEnumerable<Vault>? vaultsList = null;
        if (player.SaveData.Data.TryGetValue("vaults", out object value))
        {
            if (value is IEnumerable<Vault> v)
            {
                vaultsList = v;
            }
            else
            {
                vaultsList = new List<Vault>();
            }
        }

        return vaultsList ?? new List<Vault>();
    }

    private void AddVaults(KronstadtPlayer player, Rank rank)
    {
        Dictionary<string, Vault> vaults = GetVaults(player).ToDictionary(x => x.Name);
        byte max = (byte)rank;
        max++;
        for (int i = 0; i < max; i++)
        {
            vaults.TryAdd(Ranks[i], new(Ranks[i]));
        }

        player.SaveData.Data.AddOrUpdate("vaults", vaults.Select(x => x.Value));
    }

    private void BindVaults(KronstadtPlayer player)
    {
        IEnumerable<Vault> vaults = GetVaults(player);
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
            AddVaults(player, rank);
            BindVaults(player);
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

        player.SaveData.Data.AddOrUpdate("vaults", vaults);
    }
}
