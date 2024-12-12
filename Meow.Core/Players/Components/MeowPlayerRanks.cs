using Cysharp.Threading.Tasks;
using Meow.Core.Ranks;

namespace Meow.Core.Players.Components;

public class MeowPlayerRank
{
    public readonly MeowPlayer Owner;

    public MeowPlayerRank(MeowPlayer owner)
    {
        Owner = owner;
    }

    public async UniTask<Rank> GetRankAsync()
    {
        return await RankManager.GetRankAsync(Owner.SteamID);
    }

    public async UniTask SetRankAsync(Rank newRank)
    {
        await RankManager.SetRankAsync(Owner.SteamID, newRank);
    }
}
