using Cysharp.Threading.Tasks;
using Kronstadt.Core.Commands.Framework;
using Kronstadt.Core.Players;
using Kronstadt.Core.Translations;

namespace Kronstadt.Core.Commands;

[CommandData("vanish")]
internal class VanishCommand : Command
{
    public VanishCommand(CommandContext context) : base(context)
    {
    }

    private static readonly Translation Vanished = new("Vanished", "You are now vanished");
    private static readonly Translation Unvanished = new("Unvanished", "You are now unvanished");

    public override UniTask ExecuteAsync()
    {
        Context.AssertPlayer(out KronstadtPlayer caller);
        Context.AssertPermission("vanish");
        Context.AssertOnDuty();

        caller.Administration.VanishMode = !caller.Administration.VanishMode;
        throw Context.Reply(caller.Administration.VanishMode ? Vanished : Unvanished);
    }
}
