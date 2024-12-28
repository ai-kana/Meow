using Cysharp.Threading.Tasks;
using Meow.Core.Chat;
using Meow.Core.Fishing;
using Meow.Core.Stats;
using Meow.Core.Translations;
using SDG.Unturned;
using UnityEngine;

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
        MeowPlayerManager.OnPlayerKilled += OnPlayerKilled;
        FishingManager.OnFishCaught += OnFishCaught;
    }

    ~MeowPlayerStats()
    {
        MeowPlayerManager.OnPlayerKilled -= OnPlayerKilled;
        FishingManager.OnFishCaught -= OnFishCaught;
    }

    private void OnFishCaught(MeowPlayer catcher)
    {
        if (catcher == Owner)
        {
            AddFish();
        }
    }

    private void OnPlayerKilled(MeowPlayer victim, MeowPlayer killer)
    {
        if (victim == Owner)
        {
            AddDeath();
            return;
        }

        if (killer == Owner)
        {
            AddKill();
            return;
        }
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
        HandleKillStreak();
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

    private static readonly Translation KillStreak = new("KillStreak");

    private uint KillStreakCount = 0;
    private float LastKillTime = 0;
    private void HandleKillStreak()
    {
        float timeDifference = Time.time - LastKillTime;
        if (timeDifference > 3f)
        {
            KillStreakCount = 0;
        }

        KillStreakCount++;
        LastKillTime = Time.time;

        if (KillStreakCount > 1)
        {
            MeowChat.BroadcastMessage(KillStreak, Owner.Name, KillStreakCount);
        }
    }

    public async UniTask CommitStatsAsync()
    {
        await StatsManager.CommitSession(Owner.SteamID, ServerSession);
    }

    public async UniTask<PlayerStats?> GetStatsAsync()
    {
        return await StatsManager.GetStats(Owner.SteamID);
    }
}
