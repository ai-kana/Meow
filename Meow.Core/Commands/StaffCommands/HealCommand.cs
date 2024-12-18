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

    private static readonly Translation HealedOther = new("HealedOther");
    private static readonly Translation HealedSelf = new("HealedSelf");
    
    public override UniTask ExecuteAsync()
    {
        Context.AssertPermission("heal");

        if (Context.HasExactArguments(1))
        {
            Context.AssertPermission("heal.other");
            MeowPlayer target = Context.Parse<MeowPlayer>();
            target.Life.Heal();
            throw Context.Reply(HealedOther, target.Name);
        }
        
        Context.AssertPlayer(out MeowPlayer self);
        self.Life.Heal();
        throw Context.Reply(HealedSelf);
    }
}
