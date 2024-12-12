using Cysharp.Threading.Tasks;
using Meow.Core.Commands.Framework;
using Meow.Core.Players;
using Meow.Core.Translations;

namespace Meow.Core.Commands.StaffCommands;

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
        
        Context.AssertPlayer(out MeowPlayer self);
        MeowPlayer target = Context.Parse<MeowPlayer>();
        
        target.Moderation.Spy(self);
        throw Context.Reply(TranslationList.SpyingOn, target.Name);
    }
}
