using Cysharp.Threading.Tasks;
using Meow.Core.Chat;
using Meow.Core.Commands.Framework;
using Meow.Core.Formatting;
using Meow.Core.Offenses;
using Meow.Core.Players;
using Meow.Core.Translations;
using Steamworks;

namespace Meow.Core.Commands;

[CommandData("mute")]
[CommandSyntax("[<Params: player>] [<Params: time>] [<Params: reason...>]")]
internal class MuteCommand : Command
{
    public MuteCommand(CommandContext context) : base(context)
    {
    }

    private static readonly Translation PlayerMuteTemp = new("PlayerMuteTemp");
    private static readonly Translation PlayerMutePerm = new("PlayerMutePerm");

    public override async UniTask ExecuteAsync()
    {
        Context.AssertPermission("mute");
        Context.AssertOnDuty();
        Context.AssertArguments(3);

        CSteamID id = CSteamID.Nil;
        MeowPlayer? player = null;
        if (Context.TryParse<MeowPlayer>(out player))
        {
            id = player.SteamID;
        } 
        else 
        {
            id = Context.Parse<CSteamID>();
        }

        Context.MoveNext();
        long length = (long)Context.Parse<TimeSpan>().TotalSeconds;

        Context.MoveNext();
        string reason = Context.Form();

        if (player != null)
        {
            player.Moderation.Mute(Context.Caller.SteamID, length, reason);
        }
        else
        {
            await OffenseManager.AddOffense(Offense.Create(OffenseType.Mute, id, Context.Caller.SteamID, reason, length));
        }

        string name = player?.Name ?? id.ToString();

        MeowChat.BroadcastMessage(length == 0 ? PlayerMutePerm : PlayerMuteTemp, name, reason, Formatter.FormatTime(length));
        throw Context.Exit;
    }
}
