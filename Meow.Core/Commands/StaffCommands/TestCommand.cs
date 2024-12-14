using Cysharp.Threading.Tasks;
using Meow.Core.Commands.Framework;
using Meow.Core.Players;

namespace Meow.Core.Commands.StaffCommands;

[CommandData("test")]
internal class TestCommand : Command
{
    public TestCommand(CommandContext context) : base(context)
    {
    }

    public override UniTask ExecuteAsync()
    {
        Context.AssertPermission("all");
        Context.AssertPlayer(out MeowPlayer caller);
        caller.Stats.AddKill();
        throw new();
    }
}
