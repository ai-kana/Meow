namespace Meow.Core.Players;

public class PlayerState
{
    public readonly PlayerData SaveData;
    public PlayerState(PlayerData data)
    {
        SaveData = data;
    }

    public MeowPlayer.PlayerStats CachedStats {get; set;} = null!;

    // <summary>Stats for from logon to logoff</summary>
    public Session ServerSession {get; set;} = new();
    /// <summary>Stats for current life</summary>
    public Session LifeSession {get; set;} = new();

    public uint KillStreakCount = 0;
    public float LastKillTime = 0;

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

    public float LastCommand = 0;
}
