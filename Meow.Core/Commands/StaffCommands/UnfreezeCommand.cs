using Cysharp.Threading.Tasks;
using Meow.Core.Commands.Framework;
using Meow.Core.Players;
using Meow.Core.Translations;

namespace Meow.Core.Commands.StaffCommands;

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
        
        MeowPlayer player = Context.Parse<MeowPlayer>();
        
        if(!player.Movement.IsFrozen)
            throw Context.Reply(TranslationList.PlayerNotFrozen, player.Name);
        
        player.Movement.Unfreeze();
        throw Context.Reply(TranslationList.PlayerUnfrozen, player.Name);
    }
}
