using Cysharp.Threading.Tasks;
using Kronstadt.Core.Commands.Framework;
using Kronstadt.Core.Players;

namespace Kronstadt.Core.Commands.StaffCommands;

[CommandData("exit")]
internal class ExitCommand : Command
{
    public ExitCommand(CommandContext context) : base(context)
    {
    }

    public override UniTask ExecuteAsync()
    {
        Context.AssertPermission("exit");
        Context.AssertPlayer(out KronstadtPlayer self);
        self.Moderation.Kick();
        throw Context.Exit;
    }
}
