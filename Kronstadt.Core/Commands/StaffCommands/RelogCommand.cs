using Cysharp.Threading.Tasks;
using Kronstadt.Core.Commands.Framework;
using Kronstadt.Core.Players;
using Kronstadt.Core.Translations;
using SDG.Unturned;
using Command = Kronstadt.Core.Commands.Framework.Command;

namespace Kronstadt.Core.Commands;

[CommandData("relog")]
internal class RelogCommand : Command
{
    public RelogCommand(CommandContext context) : base(context)
    {
    }

    private static readonly Translation RelogOther = new("RelogOther", "Forced {0} to relog");

    public override UniTask ExecuteAsync()
    {
        Context.AssertPermission("relog");
        Context.AssertOnDuty();

        KronstadtPlayer target;
        if (Context.HasArguments(1))
        {
            target = Context.Parse<KronstadtPlayer>();
        }
        else
        {
            Context.AssertPlayer(out target);
        }

        target.Player.sendRelayToServer(Provider.ip, Provider.port, "", false);
        throw Context.HasArguments(1) ? Context.Reply(RelogOther, target.Name) : Context.Exit;
    }
}
