using Cysharp.Threading.Tasks;
using Meow.Core.Commands.Framework;
using Meow.Core.Players;
using Meow.Core.Ranks;

namespace Meow.Core.Commands.StaffCommands;

[CommandData("test")]
internal class TestCommand : Command
{
    public TestCommand(CommandContext context) : base(context)
    {
    }

    public override async UniTask ExecuteAsync()
    {
        Context.AssertPermission("all");
        Context.AssertPlayer(out MeowPlayer caller);
        
        Context.Reply($"Rank: {(await caller.Rank.GetRankAsync()).ToString()}");
        await caller.Rank.SetRankAsync(Rank.ProPlus);

        Context.Reply($"Rank: {(await caller.Rank.GetRankAsync()).ToString()}");
    }
}
