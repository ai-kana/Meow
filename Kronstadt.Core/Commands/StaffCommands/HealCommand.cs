using Cysharp.Threading.Tasks;
using Kronstadt.Core.Commands.Framework;
using Kronstadt.Core.Players;
using Kronstadt.Core.Translations;

namespace Kronstadt.Core.Commands;

[CommandData("heal")]
[CommandSyntax("<[player?]>")]
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
            KronstadtPlayer target = Context.Parse<KronstadtPlayer>();
            target.Life.Heal();
            throw Context.Reply(TranslationList.HealedOther, target.Name);
        }
        
        Context.AssertPlayer(out KronstadtPlayer self);
        self.Life.Heal();
        throw Context.Reply(TranslationList.HealedSelf);
    }
}
