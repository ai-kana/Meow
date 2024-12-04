using Cysharp.Threading.Tasks;
using Kronstadt.Core.Commands.Framework;
using Kronstadt.Core.Players;
using Kronstadt.Core.Ranks;

namespace Kronstadt.Core.Commands.StaffCommands;

[CommandData("test")]
internal class TestCommand : Command
{
    public TestCommand(CommandContext context) : base(context)
    {
    }

    public override async UniTask ExecuteAsync()
    {
        Context.AssertPermission("all");
        Context.AssertPlayer(out KronstadtPlayer caller);
        
        Context.Reply($"Rank: {(await caller.Rank.GetRankAsync()).ToString()}");
        await caller.Rank.SetRankAsync(Rank.ProPlus);

        Context.Reply($"Rank: {(await caller.Rank.GetRankAsync()).ToString()}");
    }
}
