using Cysharp.Threading.Tasks;
using Meow.Core.Stats;
using SDG.Unturned;

namespace Meow.Core.Players.Components;

public class PlayerStats
{
    public uint Kills {get;set;} = 0;
    public uint Deaths {get;set;} = 0;
    public uint Fish {get;set;} = 0;
    public uint ItemsLooted {get;set;} = 0;
    public ulong PlayTime {get;set;} = 0;
}

public class MeowPlayerStats
{
    public MeowPlayer Owner;
    public MeowPlayerStats(MeowPlayer owner)
    {
        Owner = owner;
        Player.onPlayerStatIncremented += OnStatTicked;
        owner.Life.OnPlayerDied += OnPlayerDied;
    }

    ~MeowPlayerStats()
    {
        Player.onPlayerStatIncremented += OnStatTicked;
    }

    private void OnPlayerDied()
    {
        OnStatTicked(Owner.Player, EPlayerStat.DEATHS_PLAYERS);
    }

    private void OnStatTicked(Player player, EPlayerStat stat)
    {
        if (player.channel.owner.playerID.steamID != Owner.SteamID)
        {
            return;
        }

        switch (stat)
        {
            case EPlayerStat.FOUND_ITEMS:
                AddItemLooted();
                break;
            case EPlayerStat.FOUND_FISHES:
                AddFish();
                break;
            case EPlayerStat.KILLS_PLAYERS:
                AddKill();
                break;
            case EPlayerStat.DEATHS_PLAYERS:
                AddDeath();
                break;
        }
    }

    public class Session
    {
        public Session()
        {
            StartTime = DateTimeOffset.Now.ToUnixTimeSeconds();
        }

        public uint Kills {get;set;} = 0;
        public uint Deaths {get;set;} = 0;
        public uint Fish {get;set;} = 0;
        public uint ItemsLooted {get;set;} = 0;
        public readonly long StartTime;
    }


    /// <summary>Stats for from logon to logoff</summary>
    public Session ServerSession {get; private set;} = new();
    /// <summary>Stats for current life</summary>
    public Session LifeSession {get;private set;} = new();

    public void StartNewLife()
    {
        LifeSession = new();
    }

    public void AddKill()
    {
        LifeSession.Kills++;
        ServerSession.Kills++;
    }

    public void AddDeath()
    {
        LifeSession.Deaths++;
        ServerSession.Deaths++;
        StartNewLife();
    }

    public void AddFish()
    {
        LifeSession.Fish++;
        ServerSession.Fish++;
    }

    public void AddItemLooted()
    {
        LifeSession.ItemsLooted++;
        ServerSession.ItemsLooted++;
    }

    public async UniTask CommitStatsAsync()
    {
        await StatsManager.CommitSession(Owner, ServerSession);
    }

    public async UniTask<PlayerStats?> GetStatsAsync()
    {
        return await StatsManager.GetStats(Owner.SteamID);
    }
}
