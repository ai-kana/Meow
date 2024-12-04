using Cysharp.Threading.Tasks;
using Kronstadt.Core.Commands.Framework;
using Kronstadt.Core.Players;
using Kronstadt.Core.Translations;
using Kronstadt.Unturnov.Doors;

namespace Kronstadt.Core.Commands;

[CommandData("doortp")]
public class DoorTpCommand : Command
{
    public DoorTpCommand(CommandContext context) : base(context)
    {
    }

    private static readonly Translation TeleportedToDoor = new("TeleportedToDoor", "You have been teleport to your door");
    private static readonly Translation NoDoorFound = new("NoDoorFound", "You do not have a door");

    public override UniTask ExecuteAsync()
    {
        Context.AssertPlayer(out KronstadtPlayer caller);
        Context.AssertCooldown();
        
        if (!caller.SaveData.Data.ContainsKey(DoorManager.DoorPositionKey))
        {
            throw Context.Reply(NoDoorFound);
        }

        if (caller.SaveData.Data[DoorManager.DoorPositionKey] is not Position position)
        {
            throw Context.Reply(NoDoorFound);
        }

        caller.Movement.Teleport(position.ToVector3());
        Context.AddCooldown(5);

        throw Context.Reply(TeleportedToDoor);
    }
}
