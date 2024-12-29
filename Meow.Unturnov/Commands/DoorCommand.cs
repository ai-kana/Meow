using Cysharp.Threading.Tasks;
using Meow.Core.Commands.Framework;
using Meow.Core.Players;
using Meow.Core.Translations;
using Microsoft.Extensions.Configuration;

namespace Meow.Unturnov.Commands;

[CommandData("door")]
internal class DoorCommand : Command
{
    public DoorCommand(CommandContext context) : base(context)
    {
    }

    private static readonly Translation GaveDoor = new("GaveDoor");

    public override UniTask ExecuteAsync()
    {
        Context.AssertPlayer(out MeowPlayer caller);
        Context.AssertCooldown();

        ushort doorId = UnturnovPlugin.Configuration.GetValue<ushort>("DoorId");
        caller.GiveItem(doorId);

        Context.AddCooldown((long)new TimeSpan(12, 0, 0).TotalSeconds);
        throw Context.Reply(GaveDoor);
    }
}

