using Cysharp.Threading.Tasks;
using Meow.Core.Commands.Framework;
using Meow.Core.Players;
using Meow.Core.Translations;

namespace Meow.Core.Commands;

[CommandData("heal")]
[CommandSyntax("[<Params: player>]")]
internal class HealCommand : Command
{
    public HealCommand(CommandContext context) : base(context)
    {
    }
    
    public override UniTask ExecuteAsync()
    {
        Context.AssertPermission("heal");

        if (Context.HasExactArguments(1))
        {
            Context.AssertPermission("heal.other");
            MeowPlayer target = Context.Parse<MeowPlayer>();
            target.Life.Heal();
            throw Context.Reply(TranslationList.HealedOther, target.Name);
        }
        
        Context.AssertPlayer(out MeowPlayer self);
        self.Life.Heal();
        throw Context.Reply(TranslationList.HealedSelf);
    }
}
