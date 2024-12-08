using Cysharp.Threading.Tasks;
using Kronstadt.Core.Commands.Framework;
using Kronstadt.Core.Players;

namespace Kronstadt.Core.Commands;

[CommandData("vanish")]
internal class VanishCommand : Command
{
    public VanishCommand(CommandContext context) : base(context)
    {
    }

    public override UniTask ExecuteAsync()
    {
        Context.AssertPlayer(out KronstadtPlayer caller);
        Context.AssertPermission("vanish");
        Context.AssertOnDuty();

        caller.Administration.VanishMode = !caller.Administration.VanishMode;
        throw Context.Reply($"Add translation here later, {caller.Administration.VanishMode}");
    }
}
