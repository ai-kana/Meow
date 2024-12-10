using Cysharp.Threading.Tasks;
using Kronstadt.Core.Commands.Framework;
using Kronstadt.Core.Players;
using Kronstadt.Core.Translations;

namespace Kronstadt.Core.Commands.StaffCommands;

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
        
        KronstadtPlayer target = Context.Parse<KronstadtPlayer>();
        
        target.Life.Kill();
        throw Context.Reply(TranslationList.Killed, target.Name);
    }
}
