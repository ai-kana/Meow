using Cysharp.Threading.Tasks;
using Kronstadt.Core.Commands.Framework;
using Kronstadt.Core.Players;
using Kronstadt.Core.Translations;

namespace Kronstadt.Core.Commands.StaffCommands;

[CommandData("spy")]
[CommandSyntax("[<Params: player>]")]
internal class SpyCommand : Command
{
    public SpyCommand(CommandContext context) : base(context)
    {
        
    }

    public override UniTask ExecuteAsync()
    {
        Context.AssertPermission("spy");
        Context.AssertOnDuty();
        Context.AssertArguments(1);
        
        Context.AssertPlayer(out KronstadtPlayer self);
        KronstadtPlayer target = Context.Parse<KronstadtPlayer>();
        
        target.Moderation.Spy(self);
        throw Context.Reply(TranslationList.SpyingOn, target.Name);
    }
}
