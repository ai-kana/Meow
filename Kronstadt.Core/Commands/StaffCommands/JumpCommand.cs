using Cysharp.Threading.Tasks;
using Kronstadt.Core.Commands.Framework;
using Kronstadt.Core.Players;
using Kronstadt.Core.Translations;
using SDG.Unturned;
using UnityEngine;
using Command = Kronstadt.Core.Commands.Framework.Command;

namespace Kronstadt.Core.Commands;

[CommandData("jump", "j")]
internal class JumpCommand : Command
{
    public JumpCommand(CommandContext context) : base(context)
    {
    }

    private async UniTask<bool> TryJump(KronstadtPlayer player, Vector3 origin, Vector3 dir)
    {
        await UniTask.Yield();

        if (!Physics.Raycast(new(origin, dir), out RaycastHit hit, 1028, RayMasks.BLOCK_COLLISION))
        {
            return false;
        }

        player.Movement.Teleport(hit.point);
        return true;
    }

    private static readonly Translation JumpNotFound = new("JumpNotFound", "You are not looking at anything");

    public override async UniTask ExecuteAsync()
    {
        Context.AssertPlayer(out KronstadtPlayer caller);
        Context.AssertPermission("jump");
        Context.AssertOnDuty();

        bool state = await TryJump(caller, caller.Player.look.transform.position, caller.Player.look.transform.forward);
        throw state ? Context.Exit : Context.Reply(JumpNotFound);
    }
}
