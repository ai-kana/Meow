using Cysharp.Threading.Tasks;
using Meow.Core.Ranks;

namespace Meow.Core.Players.Components;

public class MeowPlayerRank
{
    public readonly MeowPlayer Owner;

    public MeowPlayerRank(MeowPlayer owner)
    {
        Owner = owner;
        HandleRank().Forget();
    }

    private readonly static string[] RanksRoles = 
    [
        "vip",
        "vipplus",
        "mvp",
        "mvpplus",
    ];

    private async UniTask HandleRank()
    {
        Rank rank = await GetRankAsync();
        sbyte rankByte = (sbyte)(rank - 1);

        foreach (string role in RanksRoles)
        {
            Owner.Roles.RemoveRole(role);
        }

        if (rankByte > -1)
        {
            Owner.Roles.AddRole(RanksRoles[rankByte]);
        }
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
