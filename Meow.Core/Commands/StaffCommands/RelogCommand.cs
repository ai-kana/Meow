using Cysharp.Threading.Tasks;
using Meow.Core.Commands.Framework;
using Meow.Core.Players;
using Meow.Core.Translations;
using SDG.Unturned;
using Command = Meow.Core.Commands.Framework.Command;

namespace Meow.Core.Commands;

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

        MeowPlayer target;
        if (Context.HasArguments(1))
        {
            target = Context.Parse<MeowPlayer>();
        }
        else
        {
            Context.AssertPlayer(out target);
        }

        target.Player.sendRelayToServer(Provider.ip, Provider.port, "", false);
        throw Context.HasArguments(1) ? Context.Reply(RelogOther, target.Name) : Context.Exit;
    }
}
