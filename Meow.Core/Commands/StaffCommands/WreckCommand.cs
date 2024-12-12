using Cysharp.Threading.Tasks;
using Meow.Core.Commands.Framework;
using Meow.Core.Players;
using Meow.Core.Translations;
using SDG.Unturned;
using UnityEngine;
using Command = Meow.Core.Commands.Framework.Command;

namespace Meow.Core.Commands;

[CommandData("wreck", "w")]
internal class WreckCommand : Command
{
    public WreckCommand(CommandContext context) : base(context)
    {
    }

    private static readonly Translation FailedToFind = new("FailedToFind", "Failed to find barricade");

    private async UniTask<bool> TryDestroy(Vector3 origin, Vector3 dir)
    {
        await UniTask.Yield();

        if (!Physics.Raycast(new(origin, dir), out RaycastHit hit, 64, RayMasks.BLOCK_COLLISION))
        {
            return false;
        }

        BarricadeDrop? drop = BarricadeManager.FindBarricadeByRootTransform(hit.transform.root);
        if (drop == null)
        {
            return false;
        }

        BarricadeManager.tryGetRegion(hit.transform.root, out byte x, out byte y, out ushort plant, out BarricadeRegion region);
        BarricadeManager.destroyBarricade(drop, x, y, plant);

        return true;
    }

    public override async UniTask ExecuteAsync()
    {
        Context.AssertPlayer(out MeowPlayer caller);
        Context.AssertPermission("wreck");
        Context.AssertOnDuty();

        bool state = await TryDestroy(caller.Player.look.transform.position, caller.Player.look.transform.forward);
        throw state ? Context.Exit : Context.Reply(FailedToFind);
    }
}
