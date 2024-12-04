using Cysharp.Threading.Tasks;
using Kronstadt.Core.Commands.Framework;
using Kronstadt.Core.Players;
using Kronstadt.Core.Translations;

namespace Kronstadt.Core.Commands.StaffCommands;

[CommandData("unfreeze")]
[CommandSyntax("<[player]>")]
internal class UnfreezeCommand : Command
{
    public UnfreezeCommand(CommandContext context) : base(context)
    {
    }

    public override UniTask ExecuteAsync()
    {
        Context.AssertPermission("unfreeze");
        Context.AssertOnDuty();
        Context.AssertArguments(1);
        
        KronstadtPlayer player = Context.Parse<KronstadtPlayer>();
        
        if(!player.Movement.IsFrozen)
            throw Context.Reply(TranslationList.PlayerNotFrozen, player.Name);
        
        player.Movement.Unfreeze();
        throw Context.Reply(TranslationList.PlayerUnfrozen, player.Name);
    }
}
