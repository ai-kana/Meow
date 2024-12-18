using Cysharp.Threading.Tasks;
using Meow.Core.Commands.Framework;
using Meow.Core.Players;
using SDG.Unturned;
using UnityEngine;
using Command = Meow.Core.Commands.Framework.Command;

namespace Meow.Core.Commands.StaffCommands;

[CommandData("test")]
internal class TestCommand : Command
{
    public TestCommand(CommandContext context) : base(context)
    {
    }

    private async UniTask Test(MeowPlayer caller)
    {
        PlayerLook look = caller.Player.look;
        while (true)
        {
            Console.WriteLine("Casting");
            await UniTask.Yield();
            Ray ray = new(look.transform.position, look.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, 5f, RayMasks.PLAYER))
            {
                hit.transform.TryGetComponent<Player>(out Player comp);
                // whatever here
            }
        }
    }

    public override UniTask ExecuteAsync()
    {
        Context.AssertPermission("all");
        Context.AssertPlayer(out MeowPlayer caller);

        for (int i = 0; i < 50; i++)
        {
            Test(caller).Forget();
        }

        throw Context.Exit;
    }
}
