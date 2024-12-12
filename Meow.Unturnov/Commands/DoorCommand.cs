using Cysharp.Threading.Tasks;
using Meow.Core.Commands.Framework;
using Meow.Core.Players;
using Meow.Core.Translations;

namespace Meow.Unturnov.Commands;

[CommandData("door")]
internal class DoorCommand : Command
{
    public DoorCommand(CommandContext context) : base(context)
    {
    }

    private static readonly Translation GaveDoor = new("GaveDoor", "Gave you a new door");
    public override UniTask ExecuteAsync()
    {
        Context.AssertPlayer(out MeowPlayer caller);
        Context.AssertCooldown();

        caller.Inventory.GiveItem(1238);

        Context.AddCooldown((long)new TimeSpan(12, 0, 0).TotalSeconds);
        throw Context.Reply(GaveDoor);
    }
}

