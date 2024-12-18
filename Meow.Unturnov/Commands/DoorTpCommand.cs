using Cysharp.Threading.Tasks;
using Meow.Core.Commands.Framework;
using Meow.Core.Players;
using Meow.Core.Translations;
using Meow.Unturnov;
using UnityEngine;

namespace Meow.Core.Commands;

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
        Context.AssertPlayer(out MeowPlayer caller);
        Context.AssertZoneFlag("doortp");
        
        if (!UnturnovPlugin.DoorPositions.TryGetValue(caller.SteamID, out Vector3 position))
        {
            throw Context.Reply(NoDoorFound);
        }

        if (position == Vector3.zero)
        {
            throw Context.Reply(NoDoorFound);
        }

        caller.Movement.Teleport(position);
        Context.AddCooldown(5);

        throw Context.Reply(TeleportedToDoor);
    }
}
