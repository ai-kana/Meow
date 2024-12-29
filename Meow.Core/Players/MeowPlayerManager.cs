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
using Meow.Core.Startup;
using UnityEngine;
using System.Runtime.CompilerServices;

namespace Meow.Core.Players;

public delegate void PlayerConnected(MeowPlayer player);
public delegate void PlayerDisconnected(MeowPlayer player);

public delegate void PlayerKilled(MeowPlayer victim, MeowPlayer killer);

[Startup]
public class MeowPlayerManager
{
    public static List<MeowPlayer> Players {get; private set;}

    public static event PlayerConnected? OnPlayerConnected;
    public static event PlayerDisconnected? OnPlayerDisconnected;

    public static event PlayerKilled? OnPlayerKilled;

    private static readonly ILogger _Logger;

    static MeowPlayerManager()
    {
        _Logger = LoggerProvider.CreateLogger<MeowPlayerManager>();
        Players = new();

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
        
        PlayerLife.onPlayerDied += OnPlayerDied;
    }

    private static TranslationPackage GetFriendlyLimbName(ELimb limb)
    {
        switch (limb)
        {
            case ELimb.SKULL: return new(new("LimbSkull"));
            case ELimb.SPINE: return new(new("LimbSpine"));

            case ELimb.LEFT_ARM:
            case ELimb.RIGHT_ARM:
            case ELimb.LEFT_HAND:
            case ELimb.RIGHT_HAND:
                return new(new("LimbArm"));

            case ELimb.LEFT_LEG:
            case ELimb.RIGHT_LEG:
            case ELimb.LEFT_FOOT:
            case ELimb.RIGHT_FOOT:
                return new(new("LimbLeftLeg"));

            case ELimb.LEFT_FRONT:
            case ELimb.RIGHT_FRONT:
                return new(new("LimbFront"));

            case ELimb.LEFT_BACK:
            case ELimb.RIGHT_BACK:
                return new(new("LimbBack"));
            default:
                return new(new("LimbNotFound"));
        }
    }

    private static TranslationPackage GetDeathMessage(MeowPlayer victim, EDeathCause cause, ELimb limb, MeowPlayer? killer)
    {
        object[]? args = null;
        Translation translation;
        switch (cause)
        {
            case EDeathCause.GUN:
                float distance = Vector3.Distance(victim.Movement.Position, killer!.Movement.Position);
                TranslationPackage limbName = GetFriendlyLimbName(limb);
                string gun = ((UseableGun)killer!.Player.equipment.useable).equippedGunAsset.FriendlyName;
                args = [victim.Name, killer.Name, gun, limbName, (int)distance];
                translation = new("DeathGun");
                break;
            case EDeathCause.SPIT:
            case EDeathCause.BURNING:
            case EDeathCause.BURNER:
            case EDeathCause.ACID:
                translation = new("DeathBurn");
                break;
            case EDeathCause.FOOD:
                translation = new("DeathFood");
                break;
            case EDeathCause.WATER:
                translation = new("DeathWater");
                break;
            case EDeathCause.KILL:
                translation = new("DeathAdmin");
                break;
            case EDeathCause.ARENA: 
                translation = new("DeathArena");
                break;
            case EDeathCause.BONES: 
                translation = new("DeathFall");
                break;
            case EDeathCause.MELEE: 
                string melee = ((UseableMelee)killer!.Player.equipment.useable).equippedMeleeAsset.FriendlyName;
                args = [victim.Name, killer.Name, melee];
                translation = new("DeathMelee");
                break;
            case EDeathCause.PUNCH:
                args = [victim.Name, killer!.Name];
                translation = new("DeathPunch");
                break;
            case EDeathCause.SHRED:
                translation = new("DeathShred");
                break;
            case EDeathCause.SPARK:
                translation = new("DeathSpark");
                break;
            case EDeathCause.ANIMAL:
                translation = new("DeathAnimal");
                break;
            case EDeathCause.BREATH:
                translation = new("DeathBreath");
                break;
            case EDeathCause.CHARGE:
            case EDeathCause.SPLASH:
            case EDeathCause.GRENADE:
            case EDeathCause.MISSILE:
            case EDeathCause.LANDMINE:
                translation = new("DeathExplode");
                break;
            case EDeathCause.SENTRY:
                translation = new("DeathSentry");
                break;
            case EDeathCause.INFECTION:
            case EDeathCause.ZOMBIE:
                translation = new("DeathZombie");
                break;
            case EDeathCause.BOULDER:
                translation = new("DeathCrush");
                break;
            case EDeathCause.SUICIDE:
                translation = new("DeathSuicide");
                break;
            case EDeathCause.ROADKILL:
            case EDeathCause.VEHICLE:
                translation = new("DeathVehicle");
                break;
            case EDeathCause.BLEEDING:
                translation = new("DeathBleed");
                break;
            case EDeathCause.FREEZING:
                translation = new("DeathBleed");
                break;
            default:
                translation = new("DeathDefault");
                break;
        }

        return translation.AsPackage(args ?? []);
    }

    private static void OnPlayerDied(PlayerLife sender, EDeathCause cause, ELimb limb, CSteamID instigator)
    {
        if (!TryGetPlayer(sender.player, out MeowPlayer victim))
        {
            return;
        }

        if (TryGetPlayer(instigator, out MeowPlayer? killer))
        {
            OnPlayerKilled?.Invoke(victim, killer);
        }

        MeowChat.BroadcastMessage(Players, GetDeathMessage(victim, cause, limb, killer));
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
        return Players.ToArray();
    }

    public static void KickAll(string reason)
    {
        IEnumerable<MeowPlayer> players = GetPlayerListCopy();
        foreach (MeowPlayer player in players)
        {
            player.Moderation.Kick(reason);
        }
    }

    public static void KickAll(Translation translation, params object[] args)
    {
        IEnumerable<MeowPlayer> players = GetPlayerListCopy();
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

    private static void CheckBanned(MeowPlayer player, IEnumerable<Offense> offenses, string discord)
    {
        Offense? permBan = offenses.FirstOrDefault(x => x.OffenseType == OffenseType.Ban && x.IsPermanent);
        if (permBan != null)
        {
            player.Moderation.Kick(BanPermanent, permBan.Reason, discord);
            return;
        }

        Offense? nonPermBan = offenses.Where(x => x.OffenseType == OffenseType.Ban && !x.IsPermanent && x.IsActive)
            .OrderByDescending(x => x.Remaining).FirstOrDefault();
        if (nonPermBan != null)
        {
            player.Moderation.Kick(BanTemporary, nonPermBan.Reason, Formatter.FormatTime(nonPermBan.Remaining), discord);
        }
    }

    private static void CheckMuted(MeowPlayer player, IEnumerable<Offense> offenses, string discord)
    {
        Offense? permMute = offenses.FirstOrDefault(x => x.OffenseType == OffenseType.Mute && x.IsPermanent);
        if (permMute != null)
        {
            player.SendMessage(MutePermanent, permMute.Reason, discord);
            player.Moderation.IsMuted = true;
            return;
        }

        Offense? tempMute = offenses.Where(x => x.OffenseType == OffenseType.Mute && !x.IsPermanent && x.IsActive)
            .OrderByDescending(x => x.Remaining).FirstOrDefault();
        if (tempMute != null)
        {
            player.SendMessage(MuteTemporary, tempMute.Reason, Formatter.FormatTime(tempMute.Remaining), discord);
            player.Moderation.IsMuted = true;
            player.Moderation.EnqueueUnmute(tempMute.Remaining);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void CheckOffenses(MeowPlayer player, IEnumerable<Offense> offenses)
    {
        string discord = MeowHost.Configuration.GetValue<string>("DiscordInviteLink") ?? "Failed to get discord invite";
        CheckBanned(player, offenses, discord);
        CheckMuted(player, offenses, discord);
    }

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
        CheckOffenses(player, offenses);

        MeowChat.BroadcastMessage(PlayerConnected, player.Name);
        _Logger.LogInformation($"{player.LogName} has joined the server");
        OnPlayerConnected?.Invoke(player);
    }

    public static readonly Translation PlayerConnected = new("PlayerConnected");
    public static readonly Translation PlayerDisconnected = new("PlayerDisconnected");

    private static async void OnServerConnected(CSteamID steamID)
    {
        try
        {
            await OnConnected(steamID);
        }
        catch (Exception exception)
        {
            _Logger.LogError(exception, "Exception while player connection; Kicking...");
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
