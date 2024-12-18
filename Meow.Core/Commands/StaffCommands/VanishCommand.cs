using Cysharp.Threading.Tasks;
using Meow.Core.Commands.Framework;
using Meow.Core.Players;
using Meow.Core.Translations;

namespace Meow.Core.Commands;

[CommandData("vanish")]
internal class VanishCommand : Command
{
    public VanishCommand(CommandContext context) : base(context)
    {
    }

    private static readonly Translation Vanished = new("Vanished");
    private static readonly Translation Unvanished = new("Unvanished");

    public override UniTask ExecuteAsync()
    {
        Context.AssertPlayer(out MeowPlayer caller);
        Context.AssertPermission("vanish");
        Context.AssertOnDuty();

        caller.Administration.VanishMode = !caller.Administration.VanishMode;
        throw Context.Reply(caller.Administration.VanishMode ? Vanished : Unvanished);
    }
}
