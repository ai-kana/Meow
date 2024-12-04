using System.Collections;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using SDG.Unturned;
using Steamworks;
using UnityEngine;
using Kronstadt.Core.Offenses;
using Kronstadt.Core.Translations;

namespace Kronstadt.Core.Players.Components;

public class KronstadtPlayerModeration
{
    public readonly KronstadtPlayer Owner;
    public bool IsMuted {get; set;} = false;

    public KronstadtPlayerModeration(KronstadtPlayer owner)
    {
        Owner = owner;
    }

    public void Spy(KronstadtPlayer caller)
    {
        Owner.Player.sendScreenshot(caller.SteamID, null);
    }

    private IEnumerator? _UnmuteRoutine;
    private static IEnumerator WaitForUnmute(CSteamID id, long time)
    {
        yield return new WaitForSeconds(time);
        if (KronstadtPlayerManager.TryGetPlayer(id, out KronstadtPlayer player))
        {
            player.Moderation.IsMuted = false;
            player.SendMessage(player.Moderation.IsMuted.ToString());
            player.SendMessage(TranslationList.Unmuted);
        }
    }

    public void EnqueueUnmute(long duration)
    {
        _UnmuteRoutine = WaitForUnmute(Owner.SteamID, duration);
        MainThreadWorker.EnqueueCoroutine(_UnmuteRoutine);
    }

    public void CancelUnmute()
    {
        if (_UnmuteRoutine == null)
        {
            return;
        }

        MainThreadWorker.CancelCoroutine(_UnmuteRoutine);
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
        await OffenseManager.AddOffense(Offense.Create(OffenseType.Mute, Owner.SteamID, issuer, reason, duration));
    }

    public async UniTask AddBan(CSteamID issuer, long duration, string reason)
    {
        await OffenseManager.AddOffense(Offense.Create(OffenseType.Ban, Owner.SteamID, issuer, reason, duration));
    }

    public void Ban(CSteamID issuerId)
    {
        string discordInvite = KronstadtHost.Configuration.GetValue<string>("DiscordInviteLink")!;
        Kick(TranslationList.BanPermanent, "No reason provided", discordInvite);
        _ = AddBan(issuerId, long.MaxValue , "No reason provided");
    }
    
    public void Ban(CSteamID issuerId, long duration)
    {
        string discordInvite = KronstadtHost.Configuration.GetValue<string>("DiscordInviteLink")!;
        Kick(TranslationList.BanTemporary, "No reason provided", duration, discordInvite);
        _ = AddBan(issuerId, duration, "No reason provided");
    }
    
    public void Ban(CSteamID issuerId, string reason)
    {
        string discordInvite = KronstadtHost.Configuration.GetValue<string>("DiscordInviteLink")!;
        Kick(TranslationList.BanPermanent, reason, discordInvite);
        _ = AddBan(issuerId, long.MaxValue, reason);
    }
    
    public void Ban(CSteamID issuerId, long duration, string reason)
    {
        string discordInvite = KronstadtHost.Configuration.GetValue<string>("DiscordInviteLink")!;
        Kick(TranslationList.BanTemporary, reason, duration, discordInvite);
        _ = AddBan(issuerId, duration, reason);
    }

    public void Kick()
    {
        Provider.kick(Owner.SteamID, "No reason provided");
    }

    public void Kick(string reason)
    {
        Provider.kick(Owner.SteamID, reason);
    }

    public void Kick(Translation translation, params object[] args)
    {
        Provider.kick(Owner.SteamID, translation.TranslateNoColor(Owner.Language, args));
    }

}
