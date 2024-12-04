using Cysharp.Threading.Tasks;
using Kronstadt.Core.Commands.Framework;
using Kronstadt.Core.Players;
using Kronstadt.Core.Translations;

namespace Kronstadt.Core.Commands.StaffCommands;

[CommandData("freeze")]
[CommandSyntax("<[player]>")]
internal class FreezeCommand : Command
{
    public FreezeCommand(CommandContext context) : base(context)
    {
    }

    public override UniTask ExecuteAsync()
    {
        Context.AssertPermission("freeze");
        Context.AssertOnDuty();
        Context.AssertArguments(1);
        
        KronstadtPlayer player = Context.Parse<KronstadtPlayer>();
        
        if(player.Movement.IsFrozen)
            throw Context.Reply(TranslationList.PlayerAlreadyFrozen, player.Name);
        
        player.Movement.Freeze();
        throw Context.Reply(TranslationList.PlayerFrozen, player.Name);
    }
}
