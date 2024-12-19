using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SDG.Unturned;
using Steamworks;
using Meow.Core.Chat;
using Meow.Core.Extensions;
using Meow.Core.Formatting;
using Meow.Core.Logging;
using Meow.Core.Offenses;
using Meow.Core.Translations;
using System.Net;
using Meow.Core.Ranks;

namespace Meow.Core.Players;

public delegate void PlayerConnected(MeowPlayer player);
public delegate void PlayerDisconnected(MeowPlayer player);

public class MeowPlayerManager
{
    public static List<MeowPlayer> Players {get; private set;}

    public static event PlayerConnected? OnPlayerConnected;
    public static event PlayerDisconnected? OnPlayerDisconnected;

    private static readonly ILogger _Logger;

    static MeowPlayerManager()
    {
        _Logger = LoggerProvider.CreateLogger<MeowPlayerManager>();
        Players = new();
    }

    internal static void Load()
    {
        Provider.onServerConnected += OnServerConnected;
        Provider.onServerDisconnected += OnServerDisconnected;

        // God mode
        DamageTool.damagePlayerRequested += OnDamageRequested;
        PlayerLife.OnTellHealth_Global += GodModeHandler;
        PlayerLife.OnTellFood_Global += GodModeHandler;
        PlayerLife.OnTellWater_Global += GodModeHandler;
        PlayerLife.OnTellVirus_Global += GodModeHandler;
        PlayerLife.OnTellBroken_Global += GodModeHandler;
        PlayerLife.OnTellBleeding_Global += GodModeHandler;
        PlayerVoice.onRelayVoice += OnRelayVoice;
    }

    private static void OnRelayVoice(PlayerVoice speaker, bool wantsToUseWalkieTalkie, ref bool shouldAllow, ref bool shouldBroadcastOverRadio, ref PlayerVoice.RelayVoiceCullingHandler cullingHandler)
    {
        TryGetPlayer(speaker.player, out MeowPlayer player);
        shouldAllow = !player.Moderation.IsMuted;
    }

    private static void GodModeHandler(PlayerLife life)
    {
        // Do this with a patch later
        TryGetPlayer(life.player, out MeowPlayer player);
        if (player.Administration.GodMode)
        {
            life.sendRevive();
        }
    }

    private static void OnDamageRequested(ref DamagePlayerParameters parameters, ref bool shouldAllow)
    {
        TryGetPlayer(parameters.player, out MeowPlayer player);
        shouldAllow = !player?.Administration.GodMode ?? true;
    }

    private static IEnumerable<MeowPlayer> GetPlayerListCopy()
    {
        foreach (MeowPlayer player in Players)
        {
            yield return player;
        }
    }

    public static void KickAll(string reason)
    {
        MeowPlayer[] players = new MeowPlayer[Players.Count];
        Players.CopyTo(players);
        foreach (MeowPlayer player in players)
        {
            player.Moderation.Kick(reason);
        }
    }

    public static void KickAll(Translation translation, params object[] args)
    {
        MeowPlayer[] players = new MeowPlayer[Players.Count];
        Players.CopyTo(players);
        foreach (MeowPlayer player in players)
        {
            player.Moderation.Kick(translation, args);
        }
    }

    private static bool TryFindPlayer(CSteamID steamId, out MeowPlayer player)
    {
        player = null!;
        for (int i = 0; i < Players.Count; i++)
        {
            player = Players[i];
            if (player.SteamID == steamId)
            {
                return true;
            }
        }

        return false;
    }

    public static bool TryGetPlayer(Player inPlayer, out MeowPlayer player)
    {
        return TryFindPlayer(inPlayer.channel.owner.playerID.steamID, out player);
    }

    public static bool TryGetPlayer(SteamPlayer inPlayer, out MeowPlayer player)
    {
        return TryFindPlayer(inPlayer.playerID.steamID, out player);
    }

    public static bool TryGetPlayer(CSteamID id, out MeowPlayer player)
    {
        return TryFindPlayer(id, out player);
    }
    
    public static bool IsOnline(CSteamID steamID)
    {
        return TryFindPlayer(steamID, out _);
    }

    public static bool TryFindPlayer(string search, out MeowPlayer player)
    {
        if (PlayerTool.tryGetSteamID(search, out CSteamID steamID))
        {
            return TryGetPlayer(steamID, out player);
        }

        player = MeowPlayerManager.Players.FirstOrDefault(x => x.Name.Contains(search, StringComparison.OrdinalIgnoreCase));
        if (player == null)
        {
            return false;
        }

        return true;
    }

    public static void Relog(MeowPlayer player)
    {
        Relog(player.SteamPlayer);
    }

    public static void Relog(SteamPlayer player)
    {
        IConfigurationSection section = MeowHost.Configuration.GetRequiredSection("RelayInfo");
        string addrStr = section.GetValue<string>("IP") ?? throw new("Failed to get IP address value");
        IPAddress address = IPAddress.Parse(addrStr);
        uint realAddr = BitConverter.ToUInt32(address.MapToIPv4().GetAddressBytes(), 0);
        ushort port = section.GetValue<ushort>("Port");
        _Logger.LogInformation($"Recon: {address.MapToIPv4()}, {port}");

        SendRelayToServer(player.player, Provider.ip, Provider.port);
    }

    public static void SendRelayToServer(MeowPlayer player, uint ip, ushort port, string password = null!)
    {
        password ??= "";
        player.Player.sendRelayToServer(ip, port, password, false);
    }

    public static void SendRelayToServer(Player player, uint ip, ushort port)
    {
        player.sendRelayToServer(ip, port, Provider.serverPassword, false);
    }

    private static readonly Translation MutePermanent = new("MutePermanent");
    private static readonly Translation MuteTemporary = new("MuteTemporary");

    private static readonly Translation BanPermanent = new("BanPermanent");
    private static readonly Translation BanTemporary = new("BanTemporary");

    private static async UniTask OnConnected(CSteamID steamID)
    {
        SteamPlayer steamPlayer = Provider.clients.Find(x => x.playerID.steamID == steamID);
        uint ip = steamPlayer.getIPv4AddressOrZero();
        if (ip == 0)
        {
            Provider.kick(steamPlayer.playerID.steamID, "Do not connect using the server code");
            return;
        }

        MeowPlayer player = await MeowPlayer.CreateAsync(steamPlayer);
        Players.Add(player);

        IEnumerable<Offense> offenses = await PlayerIdManager.GetOffensesAsync(player);

        string discord = MeowHost.Configuration.GetValue<string>("DiscordInviteLink") ?? "Failed to get discord link";

        Offense? permBan = offenses.FirstOrDefault(x => x.OffenseType == OffenseType.Ban && x.Duration == 0 && !x.Pardoned);
        if (permBan != null)
        {
            player.Moderation.Kick(BanPermanent, permBan.Reason, discord);
            return;
        }
        else
        {
            Offense? nonPermBan = offenses.Where(x => x.OffenseType == OffenseType.Ban && !x.IsPermanent && x.IsActive)
                .OrderByDescending(x => x.Remaining).FirstOrDefault();
            if (nonPermBan != null)
            {
                player.Moderation.Kick(BanTemporary, nonPermBan.Reason, Formatter.FormatTime(nonPermBan.Remaining), discord);
                return;
            }
        }

        Offense? permMute = offenses.FirstOrDefault(x => x.OffenseType == OffenseType.Mute && x.IsPermanent);
        if (permMute != null)
        {
            player.SendMessage(MutePermanent, permMute.Reason, discord);
            player.Moderation.IsMuted = true;
        }
        else
        {
            Offense? tempMute = offenses.Where(x => x.OffenseType == OffenseType.Mute && !x.IsPermanent && x.IsActive)
                .OrderByDescending(x => x.Remaining).FirstOrDefault();
            if (tempMute != null)
            {
                player.SendMessage(MuteTemporary, tempMute.Reason, Formatter.FormatTime(tempMute.Remaining), discord);
                player.Moderation.IsMuted = true;
                player.Moderation.EnqueueUnmute(tempMute.Remaining);
            }
        }

        Rank rank = await player.Rank.GetRankAsync();
        sbyte rankByte = (sbyte)(rank - 1);

        foreach (string role in RanksRoles)
        {
            player.Roles.RemoveRole(role);
        }

        if (rankByte > -1)
        {
            player.Roles.AddRole(RanksRoles[rankByte]);
        }

        MeowChat.BroadcastMessage(PlayerConnected, player.Name);
        _Logger.LogInformation($"{player.LogName} has joined the server");
        OnPlayerConnected?.Invoke(player);
    }

    private readonly static string[] RanksRoles = 
    [
        "vip",
        "vipplus",
        "mvp",
        "mvpplus",
    ];

    public static readonly Translation PlayerConnected = new("PlayerConnected");
    public static readonly Translation PlayerDisconnected = new("PlayerDisconnected");

    private static async void OnServerConnected(CSteamID steamID)
    {
        try
        {
            await OnConnected(steamID);
        }
        catch (Exception ex)
        {
            _Logger.LogError(ex, "Exception while player connection; Kicking...");
            string discord = MeowHost.Configuration.GetValue<string>("DiscordInviteLink") ?? "Failed to get discord link";
            Provider.kick(steamID, $"Something failed while connecting; Please contact staff; {discord ?? "Failed to get link :C"}");
        }
    }

    private static async void OnServerDisconnected(CSteamID steamID)
    {
        if (!TryGetPlayer(steamID, out MeowPlayer player))
        {
            return;
        }

        if (player.Administration.FakedDisconnected)
        {
           return;
        }

        OnPlayerDisconnected?.Invoke(player);

        Players.Remove(player);
        await PlayerDataManager.SaveDataAsync(player);

        player.Moderation.CancelUnmute();

        MeowChat.BroadcastMessage(PlayerDisconnected, player.Name);
        _Logger.LogInformation($"{player.LogName} has left the server");
    }
}
