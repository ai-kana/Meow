using Cysharp.Threading.Tasks;
using Meow.Core.Commands.Framework;
using Meow.Core.Players;
using Meow.Core.Translations;
using Command = Meow.Core.Commands.Framework.Command;

namespace Meow.Core.Commands.StaffCommands;

[CommandData("kick")]
[CommandSyntax("[<Params: player>] [<Params: reason...>]")]
internal class KickCommand : Command
{
    public KickCommand(CommandContext context) : base(context)
    {
    }

    public override UniTask ExecuteAsync()
    {
        Context.AssertPermission("kick");
        Context.AssertOnDuty();
        Context.AssertArguments(1);
        
        MeowPlayer target = Context.Parse<MeowPlayer>();

        if (Context.HasExactArguments(1))
        {
            target.Moderation.Kick();
            throw Context.Reply(TranslationList.Kicked, target.Name);
        }
        
        Context.MoveNext();
        string reason = Context.Form();
        
        target.Moderation.Kick(reason);
        throw Context.Reply(TranslationList.KickedReason, target.Name, reason);
    }
}
