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

    private static readonly Translation PlayerNotFrozen = new("PlayerAlreadyUnfrozen");
    private static readonly Translation PlayerUnfrozen = new("PlayerUnfrozen");

    public override UniTask ExecuteAsync()
    {
        Context.AssertPermission("unfreeze");
        Context.AssertOnDuty();
        Context.AssertArguments(1);
        
        MeowPlayer player = Context.Parse<MeowPlayer>();
        
        if(!player.IsFrozen)
            throw Context.Reply(PlayerNotFrozen, player.Name);
        
        player.Unfreeze();
        throw Context.Reply(PlayerUnfrozen, player.Name);
    }
}
