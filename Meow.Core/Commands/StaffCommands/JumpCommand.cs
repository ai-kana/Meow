using Cysharp.Threading.Tasks;
using Meow.Core.Commands.Framework;
using Meow.Core.Players;
using Meow.Core.Translations;
using SDG.Unturned;
using UnityEngine;
using Command = Meow.Core.Commands.Framework.Command;

namespace Meow.Core.Commands;

[CommandData("jump", "j")]
internal class JumpCommand : Command
{
    public JumpCommand(CommandContext context) : base(context)
    {
    }

    private async UniTask<bool> TryJump(MeowPlayer player, Vector3 origin, Vector3 dir)
    {
        await UniTask.Yield();

        if (!Physics.Raycast(new(origin, dir), out RaycastHit hit, 1028, RayMasks.BLOCK_COLLISION))
        {
            return false;
        }

        player.Teleport(hit.point);
        return true;
    }

    private static readonly Translation JumpNotFound = new("JumpNotFound");

    public override async UniTask ExecuteAsync()
    {
        Context.AssertPlayer(out MeowPlayer caller);
        Context.AssertPermission("jump");
        Context.AssertOnDuty();

        bool state = await TryJump(caller, caller.Player.look.transform.position, caller.Player.look.transform.forward);
        throw state ? Context.Exit : Context.Reply(JumpNotFound);
    }
}
