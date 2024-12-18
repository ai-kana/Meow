using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using SDG.Unturned;
using Steamworks;
using Meow.Core.Offenses;
using Meow.Core.Translations;

namespace Meow.Core.Players.Components;

public class MeowPlayerModeration
{
    public readonly MeowPlayer Owner;
    public bool IsMuted {get; set;} = false;
    private CancellationTokenSource? _UnmuteSource = null;

    public MeowPlayerModeration(MeowPlayer owner)
    {
        Owner = owner;
        MeowPlayerManager.OnPlayerDisconnected += OnDisconnected;
    }

    ~MeowPlayerModeration()
    {
        MeowPlayerManager.OnPlayerDisconnected -= OnDisconnected;
    }

    private void OnDisconnected(MeowPlayer player)
    {
        _UnmuteSource?.Cancel();
    }

    public void Spy(MeowPlayer caller)
    {
        Owner.Player.sendScreenshot(caller.SteamID, null);
    }

    public static readonly Translation Unmuted = new("Unmuted");
    private static async UniTask WaitForUnmute(CSteamID id, long time, CancellationToken token)
    {
        await UniTask.Delay((int)(time * 1000), cancellationToken: token);
        if (token.IsCancellationRequested)
        {
            return;
        }

        if (MeowPlayerManager.TryGetPlayer(id, out MeowPlayer player))
        {
            player.Moderation.IsMuted = false;
            player.SendMessage(Unmuted);
        }
    }

    public void EnqueueUnmute(long duration)
    {
        _UnmuteSource = new();
        WaitForUnmute(Owner.SteamID, duration, _UnmuteSource.Token).Forget();
    }

    public void CancelUnmute()
    {
        if (_UnmuteSource == null)
        {
            return;
        }

        _UnmuteSource.Cancel();
        _UnmuteSource = null;
    }

    public async UniTask<IEnumerable<Offense>> GetAllOffenses()
    {
        return await OffenseManager.GetOffenses(Owner.SteamID);
    }

    public async UniTask<IEnumerable<Offense>> GetWarns()
    {
        return await OffenseManager.GetWarnOffenses(Owner.SteamID);
    }

    public async UniTask AddWarn(CSteamID issuer, string reason)
    {
        await OffenseManager.AddOffense(Offense.Create(OffenseType.Warn, Owner.SteamID, issuer, reason, 0));
    }

    public async UniTask AddMute(CSteamID issuer, long duration, string reason)
    {
        if (duration != 0)
        {
            EnqueueUnmute(duration);
        }

        await OffenseManager.AddOffense(Offense.Create(OffenseType.Mute, Owner.SteamID, issuer, reason, duration));
    }

    public async UniTask AddBan(CSteamID issuer, long duration, string reason)
    {
        await OffenseManager.AddOffense(Offense.Create(OffenseType.Ban, Owner.SteamID, issuer, reason, duration));
    }

    private static readonly Translation MutePermanent = new("MutePermanent");
    private static readonly Translation MuteTemporary = new("MuteTemporary");

    private static readonly Translation BanPermanent = new("BanPermanent");
    private static readonly Translation BanTemporary = new("BanTemporary");

    public void Mute(CSteamID issuerId)
    {
        string discordInvite = MeowHost.Configuration.GetValue<string>("DiscordInviteLink")!;
        Kick(MutePermanent, "No reason provided", discordInvite);
        _ = AddMute(issuerId, 0, "No reason provided");
    }

    public void Mute(CSteamID issuerId, long duration)
    {
        string discordInvite = MeowHost.Configuration.GetValue<string>("DiscordInviteLink")!;
        _ = AddMute(issuerId, duration, "No reason provided");
    }

    public void Mute(CSteamID issuerId, long duration, string reason)
    {
        string discordInvite = MeowHost.Configuration.GetValue<string>("DiscordInviteLink")!;
        _ = AddMute(issuerId, duration, reason);
    }

    public void Ban(CSteamID issuerId)
    {
        string discordInvite = MeowHost.Configuration.GetValue<string>("DiscordInviteLink")!;
        Kick(BanPermanent, "No reason provided", discordInvite);
        _ = AddBan(issuerId, 0, "No reason provided");
    }
    
    public void Ban(CSteamID issuerId, long duration)
    {
        string discordInvite = MeowHost.Configuration.GetValue<string>("DiscordInviteLink")!;
        Kick(BanTemporary, "No reason provided", duration, discordInvite);
        _ = AddBan(issuerId, duration, "No reason provided");
    }
    
    public void Ban(CSteamID issuerId, string reason)
    {
        string discordInvite = MeowHost.Configuration.GetValue<string>("DiscordInviteLink")!;
        Kick(BanPermanent, reason, discordInvite);
        _ = AddBan(issuerId, long.MaxValue, reason);
    }
    
    public void Ban(CSteamID issuerId, long duration, string reason)
    {
        string discordInvite = MeowHost.Configuration.GetValue<string>("DiscordInviteLink")!;
        Kick(BanTemporary, reason, duration, discordInvite);
        _ = AddBan(issuerId, duration, reason);
    }

    private async UniTask DoKick(string reason)
    {
        await UniTask.Yield();
        Provider.kick(Owner.SteamID, reason);
    }

    public void Kick()
    {
        DoKick("No reason provided").Forget();
    }

    public void Kick(string reason)
    {
        DoKick(reason).Forget();
    }

    public void Kick(Translation translation, params object[] args)
    {
        Provider.kick(Owner.SteamID, translation.TranslateNoColor(Owner.Language, args));
    }

}
