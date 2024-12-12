using Cysharp.Threading.Tasks;
using Meow.Core.Commands.Framework;
using Meow.Core.Players;
using Meow.Core.Translations;

namespace Meow.Core.Commands.StaffCommands;

[CommandData("freeze")]
[CommandSyntax("[<Params: player>]")]
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
        
        MeowPlayer player = Context.Parse<MeowPlayer>();
        
        if(player.Movement.IsFrozen)
            throw Context.Reply(TranslationList.PlayerAlreadyFrozen, player.Name);
        
        player.Movement.Freeze();
        throw Context.Reply(TranslationList.PlayerFrozen, player.Name);
    }
}
