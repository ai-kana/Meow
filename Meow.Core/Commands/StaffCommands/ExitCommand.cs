using Cysharp.Threading.Tasks;
using Meow.Core.Commands.Framework;
using Meow.Core.Players;

namespace Meow.Core.Commands.StaffCommands;

[CommandData("exit")]
internal class ExitCommand : Command
{
    public ExitCommand(CommandContext context) : base(context)
    {
    }

    public override UniTask ExecuteAsync()
    {
        Context.AssertPermission("exit");
        Context.AssertPlayer(out MeowPlayer self);
        self.Kick();
        throw Context.Exit;
    }
}
