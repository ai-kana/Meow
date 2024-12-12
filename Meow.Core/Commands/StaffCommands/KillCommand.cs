using Cysharp.Threading.Tasks;
using Meow.Core.Commands.Framework;
using Meow.Core.Players;
using Meow.Core.Translations;

namespace Meow.Core.Commands.StaffCommands;

[CommandData("kill")]
[CommandSyntax("[<Params: player>]")]
internal class KillCommand : Command
{
    public KillCommand(CommandContext context) : base(context)
    {
        
    }

    public override UniTask ExecuteAsync()
    {
        Context.AssertPermission("kill");
        Context.AssertOnDuty();
        Context.AssertArguments(1);
        
        MeowPlayer target = Context.Parse<MeowPlayer>();
        
        target.Life.Kill();
        throw Context.Reply(TranslationList.Killed, target.Name);
    }
}
