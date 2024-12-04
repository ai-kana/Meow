using Cysharp.Threading.Tasks;
using Kronstadt.Core.Ranks;

namespace Kronstadt.Core.Players.Components;

public class KronstadtPlayerRank
{
    public readonly KronstadtPlayer Owner;

    public KronstadtPlayerRank(KronstadtPlayer owner)
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
