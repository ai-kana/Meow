using Cysharp.Threading.Tasks;
using Kronstadt.Core.Commands.Framework;
using Kronstadt.Core.Players;
using Kronstadt.Core.Translations;
using Command = Kronstadt.Core.Commands.Framework.Command;

namespace Kronstadt.Core.Commands.StaffCommands;

[CommandData("kick")]
[CommandSyntax("<player> <reason?>")]
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
        
        KronstadtPlayer target = Context.Parse<KronstadtPlayer>();

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
