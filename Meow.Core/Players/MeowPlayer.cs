using Cysharp.Threading.Tasks;
using SDG.Unturned;
using Steamworks;
using Meow.Core.Chat;
using Meow.Core.Fishing;
using Meow.Core.Translations;
using SDG.NetTransport;
using UnityEngine;
using Meow.Core.Zones;
using Meow.Core.Roles;
using Meow.Core.Offenses;
using Meow.Core.Ranks;
using Meow.Core.Stats;
using Meow.Core.Commands.Framework;
using Meow.Core.Extensions;
using System.Runtime.InteropServices;

namespace Meow.Core.Players;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MeowPlayer : 
    IPlayer, 
    IFormattable,
    IDisposable
{
    // All state, 16 bytes
    public SteamPlayer SteamPlayer {get; private set;}
    private readonly PlayerState _PlayerState;

    public PlayerData SaveData => _PlayerState.SaveData;

    public Player Player => SteamPlayer.player;

    public string LogName => $"{Name} ({SteamID})";

    public string Name => SteamPlayer.playerID.characterName;

    public CSteamID SteamID => SteamPlayer.playerID.steamID;

    public ITransportConnection TransportConnection => SteamPlayer.transportConnection;

    public FishingSkill FishingSkill => SaveData.Fishing;

    public string Language => SaveData.Language;

    internal static async UniTask<MeowPlayer> CreateAsync(SteamPlayer player)
    {
        return new(player, await PlayerDataManager.LoadDataAsync(player.playerID.steamID));
    }

    private MeowPlayer(SteamPlayer player, PlayerData data)
    {
        _PlayerState = new(data);
        SteamPlayer = player;

        SteamPlayer.player.interact.sendSalvageTimeOverride(2f);

        DutyStates.Add(this, false);
        HandleRank().Forget();

        MeowPlayerManager.OnPlayerKilled += OnPlayerKilled;
        FishingManager.OnFishCaught += OnFishCaught;
        SetCacheStats().Forget();
    }

    public void Dispose()
    {
        DutyStates.Remove(this);
        MeowPlayerManager.OnPlayerKilled -= OnPlayerKilled;
        FishingManager.OnFishCaught -= OnFishCaught;
    }

    public string ToString(string format, IFormatProvider formatProvider)
    {
        return Name;
    }

    public void SendMessage(string format, params object[] args)
    {
        MeowChat.BroadcastMessage(this, format, args);
    }

    public void SendMessage(Translation translation, params object[] args)
    {
        MeowChat.BroadcastMessage(this, translation.Translate(Language, args));
    }

    //
    //
    // Operators
    //
    //

    public override bool Equals(object obj)
    {
        return obj.GetHashCode() == GetHashCode();
    }

    public override int GetHashCode()
    {
        // Returns bottom half of the steam id
        // 00 00 00 00 11 11 11 11
        return (int)(SteamID.m_SteamID & (0 << 32));
    }

    public static bool operator ==(MeowPlayer p1, MeowPlayer p2)
    {
        return p1.SteamID == p2.SteamID;
    }

    public static bool operator !=(MeowPlayer p1, MeowPlayer p2)
    {
        return p1.SteamID != p2.SteamID;
    }

    //
    //
    // Movement
    //
    //

    public Vector3 Position => Player.transform.position;
    public Quaternion Rotation => Player.transform.rotation;

    private PlayerMovement _PlayerMovement => Player.movement;

    public bool IsFrozen => _PlayerMovement.pluginSpeedMultiplier == 0;

    public bool HasZoneFlag(string flag)
    {
        return ZoneManager.HasFlag(this, flag);
    }

    private async UniTask DoTeleport(Vector3 location, Quaternion rotation)
    {
        await UniTask.Yield();
        Player.teleportToLocationUnsafe(location + new Vector3(0f, 0.5f, 0f), rotation.eulerAngles.y);
    }

    public void Teleport(Vector3 location, Quaternion rotation)
    {
        DoTeleport(location, rotation).Forget();
    }

    public void Teleport(Vector3 location)
    {
        Teleport(location, Rotation);
    }

    public void Teleport(MeowPlayer player)
    {
        Teleport(Position, Rotation);
    }

    public void SetSpeed(float speed)
    {
        _PlayerMovement.sendPluginSpeedMultiplier(speed);
    }

    public void ChangeSpeed(float delta)
    {
        _PlayerMovement.sendPluginSpeedMultiplier(_PlayerMovement.pluginSpeedMultiplier + delta);
    }

    public void ResetSpeed()
    {
        _PlayerMovement.sendPluginSpeedMultiplier(1f);
    }

    public void SetJump(float jump)
    {
        _PlayerMovement.sendPluginJumpMultiplier(jump);
    }

    public void ChangeJump(float delta)
    {
        _PlayerMovement.sendPluginJumpMultiplier(_PlayerMovement.pluginSpeedMultiplier + delta);
    }

    public void ResetJump()
    {
        _PlayerMovement.sendPluginJumpMultiplier(1f);
    }

    public void SetGravity(float gravity)
    {
        _PlayerMovement.sendPluginGravityMultiplier(gravity);
    }

    public void ChangeGravity(float delta)
    {
        _PlayerMovement.sendPluginGravityMultiplier(_PlayerMovement.pluginSpeedMultiplier + delta);
    }

    public void ResetGravity()
    {
        _PlayerMovement.sendPluginGravityMultiplier(1f);
    }

    public void Freeze()
    {
        SetSpeed(0f);
        SetJump(0f);
        SetGravity(0f);
        _PlayerMovement.forceRemoveFromVehicle();
    }

    public void Unfreeze()
    {
        ResetSpeed();
        ResetJump();
        ResetGravity();
    }

    //
    //
    // Roles
    //
    //

    public HashSet<string> Roles => SaveData.Roles;

    public bool AddRole(string id)
    {
        return Roles.Add(id);
    }

    public bool RemoveRole(string id)
    {
        return Roles.Remove(id);
    }

    public bool HasRole(string id)
    {
        return Roles.Contains(id);
    }

    //
    //
    // Permissions
    //
    //

    public HashSet<string> Permissions => SaveData.Permissions;

    public void AddPermission(string permission)
    {
        Permissions.Add(permission.ToLower());
    }

    public void RemovePermission(string permission)
    {
        Permissions.Remove(permission.ToLower());
    }

    public bool HasPermission(string permission)
    {
        if (Permissions.Contains("all"))
        {
            return true;
        }

        if (Permissions.Contains(permission.ToLower()))
        {
            return true;
        }

        HashSet<Role> roles = RoleManager.GetRoles(Roles);
        foreach (Role role in roles)
        {
            if (role.Permissions.Contains("all"))
            {
                return true;
            }

            if (role.Permissions.Contains(permission, StringComparer.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    //
    //
    // Life
    //
    //

    private PlayerLife _Life => Player.life;

    public void Heal()
    {
        _Life.sendRevive();
    }

    public void Kill()
    {
        _Life.askDamage(byte.MaxValue, Vector3.up * 5, EDeathCause.KILL, ELimb.SKULL, new CSteamID(0UL), out _, false, ERagdollEffect.GOLD, true, true);
    }

    //
    //
    // Quests
    //
    //

    private PlayerQuests _Quests => Player.quests;
    private PlayerSkills _Skills => Player.skills;

    public int Reputation => _Skills.reputation;
    
    public bool TryGetMarkerPosition(out Vector3 position)
    {
        position = Vector3.zero;
        if (!_Quests.isMarkerPlaced)
        {
            return false;
        }

        Vector3 marker = _Quests.markerPosition;
        Ray ray = new(new Vector3(marker.x, Level.HEIGHT, marker.y), Vector3.down);
        if (!Physics.Raycast(ray, out RaycastHit info, Level.HEIGHT, RayMasks.BLOCK_COLLISION))
        {
            return false;
        }

        position = info.point;
        return true;
    }

    public bool FlagExists(ushort id)
    {
        return _Quests.flagsList.Exists(x => x.id == id);
    }

    public void SetFlag(ushort id, short value)
    {
        _Quests.setFlag(id, value);
    }

    public void RemoveFlag(ushort id)
    {
        _Quests.removeFlag(id);
    }

    public bool TryGetFlag(ushort id, out short value)
    {
        return _Quests.getFlag(id, out value);
    }
    
        
    public void GiveReputation(int rep)
    {
        _Skills.askRep(rep);
    }
    
    public void RemoveReputation(int rep)
    {
        _Skills.askRep(-rep);
    }
    
    public void SetReputation(int rep)
    {
        _Skills.askRep(rep - _Skills.reputation);
    }

    public bool IsInSameGroup(MeowPlayer other)
    {
        return Player.quests.isMemberOfSameGroupAs(other.Player);
    }

    //
    //
    // Cooldown
    //
    //

    private Dictionary<string, long> Cooldowns => SaveData.Cooldowns;

    public long GetCooldown(string id)
    {
        long now = DateTimeOffset.Now.ToUnixTimeSeconds();
        if (!Cooldowns.ContainsKey(id))
        {
            return 0;
        }

        long remaining = Cooldowns[id] - now;
        if (remaining < 0)
        {
            Cooldowns.Remove(id);
            return 0;
        }

        return remaining;
    }

    public void AddCooldown(string id, long length)
    {
        long end = DateTimeOffset.Now.ToUnixTimeSeconds() + length;
        if (Cooldowns.ContainsKey(id))
        {
            Cooldowns[id] = end;
            return;
        }

        Cooldowns.Add(id, end);
    }

    //
    //
    // Skills
    //
    //

    public uint Experience => _Skills.experience;

    public void GiveExperience(uint xp)
    {
        _Skills.ServerSetExperience(_Skills.experience + xp);
    }

    public void RemoveExperience(uint xp)
    {
        _Skills.ServerSetExperience(_Skills.experience - xp);
    }

    public void SetExperience(uint xp)
    {
        _Skills.ServerSetExperience(xp);
    }

    //
    //
    // Inventory
    //
    //

    private PlayerInventory _Inventory => Player.inventory;

    public void GiveItem(ushort id)
    {
        _Inventory.forceAddItem(new Item(id, EItemOrigin.ADMIN), true);
    }
    
    public void GiveItem(ushort id, byte amount, byte quality)
    {
        _Inventory.forceAddItem(new Item(id, amount, quality), true);
    }
    
    public void GiveItem(ushort id, byte amount, byte quality, byte[] state)
    {
        _Inventory.forceAddItem(new Item(id, amount, quality, state), true);
    }
    
    public void GiveItems(ushort id, ushort count)
    {
        for (int i = 0; i < count; i++)
        {
            GiveItem(id);
        }
    }
    
    public void GiveItems(ushort id, byte amount, byte quality, ushort count)
    {
        for (int i = 0; i < count; i++)
        {
            GiveItem(id, amount, quality);
        }
    }
    
    public void GiveItems(ushort id, byte amount, byte quality, byte[] state, ushort count)
    {
        for (int i = 0; i < count; i++)
        {
            GiveItem(id, amount, quality, state);
        }
    }
    
    public bool ClearInventory()
    {
        for (int page = 0; page < PlayerInventory.PAGES - 2; page++)
        {
            int count = _Inventory.getItemCount((byte)page);
            if(count == 0) continue;

            for (int index = 0; index < count; index++)
            {
                _Inventory.removeItem((byte)page, 0);
            }
        }
        
        return true;
    }
    
    public void ClearHands() 
    {
        for (byte itemCount = 0; itemCount < _Inventory.getItemCount(2); itemCount++)
        {
            _Inventory.removeItem(2, 0);
        }
    }

    //
    //
    // Clothing 
    //
    //

    private PlayerClothing _Clothing => Player.clothing;

    public bool ClearClothes()
    {
        byte[] state = [];
        
        _Clothing.askWearHat(0, 0, state, false);
        ClearHands();
        
        _Clothing.askWearGlasses(0, 0, state, false);
        ClearHands();
        
        _Clothing.askWearMask(0, 0, state, false);
        ClearHands();
        
        _Clothing.askWearShirt(0, 0, state, false);
        ClearHands();
        
        _Clothing.askWearBackpack(0, 0, state, false);
        ClearHands();
        
        _Clothing.askWearVest(0, 0, state, false);
        ClearHands();
        
        _Clothing.askWearPants(0, 0, state, false);
        ClearHands();
        
        return true;
    }

    //
    //
    // Moderation
    //
    //

    public bool IsMuted => MeowPlayerManager.IsMuted(SteamID);

    public async UniTask<IEnumerable<Offense>> GetAllOffenses()
    {
        return await OffenseManager.GetOffenses(SteamID);
    }

    public async UniTask<IEnumerable<Offense>> GetWarns()
    {
        return await OffenseManager.GetWarnOffenses(SteamID);
    }

    public async UniTask AddWarn(CSteamID issuer, string reason)
    {
        Offense offense = Offense.Create(OffenseType.Warn, SteamID, issuer, reason, 0);
        await OffenseManager.AddOffense(offense);
    }

    public void Spy(MeowPlayer caller)
    {
        Player.sendScreenshot(caller.SteamID, null);
    }

    public void Mute(CSteamID issuerId)
    {
        Mute(issuerId, 0, "No reason provided");
    }

    public void Mute(CSteamID issuerId, long duration)
    {
        Mute(issuerId, duration, "No reason provided");
    }

    public void Mute(CSteamID issuer, long duration, string reason)
    {
        MeowPlayerManager.Mute(SteamID, issuer, duration, reason);
    }

    public void Ban(CSteamID issuerId)
    {
        Ban(issuerId, 0, "No reason provided");
    }
    
    public void Ban(CSteamID issuer, long duration)
    {
        Ban(issuer, duration, "No reason provided");
    }
    
    public void Ban(CSteamID issuer, string reason)
    {
        Ban(issuer, 0, reason);
    }
    
    public void Ban(CSteamID issuer, long duration, string reason)
    {
        MeowPlayerManager.Ban(SteamID, issuer, duration, reason);
    }

    public void Kick()
    {
        MeowPlayerManager.Kick(this, "No reason provided");
    }

    public void Kick(string reason)
    {
        MeowPlayerManager.Kick(this, reason);
    }

    public void Kick(Translation translation, params object[] args)
    {
        MeowPlayerManager.Kick(this, translation.TranslateNoColor(Language, args));
    }

    //
    //
    // Administration
    //
    //

    private static Dictionary<MeowPlayer, bool> DutyStates = new();
    public bool OnDuty 
    {
        get => DutyStates[this];
        set => DutyStates[this] = value;
    }

    public bool GodMode 
    {
        get => _Life.onHealthUpdated.GetInvocationList().Contains(OnLifeUpdate);
        set => SetGodeMode(value);
    }
    private void SetGodeMode(bool state)
    {
        if (state == GodMode)
        {
            return;
        }

        if (state)
        {
            _Life.onHealthUpdated += OnLifeUpdate;
            _Life.onWaterUpdated += OnLifeUpdate;
            _Life.onFoodUpdated += OnLifeUpdate;
            _Life.onStaminaUpdated += OnLifeUpdate;
        }
        else
        {
            _Life.onHealthUpdated -= OnLifeUpdate;
            _Life.onWaterUpdated -= OnLifeUpdate;
            _Life.onFoodUpdated -= OnLifeUpdate;
            _Life.onStaminaUpdated -= OnLifeUpdate;
        }
    }

    private void OnLifeUpdate(byte damage)
    {
        Heal();
    }

    public bool VanishMode
    {
        get => !Player.movement.canAddSimulationResultsToUpdates;
        set
        {
            Player.movement.canAddSimulationResultsToUpdates = !value;
            Teleport(this);
        }
    }

    public bool ToggleDuty()
    {
        OnDuty = !OnDuty;
        SetGodeMode(OnDuty);
        VanishMode = false;

        SetSpeed(1f);
        SetJump(1f);
        SetGravity(1f);

        if (HasPermission("spectator"))
        {
            Player.look.sendFreecamAllowed(OnDuty);
            Player.look.sendWorkzoneAllowed(OnDuty);
            Player.look.sendSpecStatsAllowed(OnDuty);
        }

        return OnDuty;
    }

    //
    //
    // Ranks
    //
    //

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
            RemoveRole(role);
        }

        if (rankByte > -1)
        {
            AddRole(RanksRoles[rankByte]);
        }
    }

    public async UniTask<Rank> GetRankAsync()
    {
        return await RankManager.GetRankAsync(SteamID);
    }

    public async UniTask SetRankAsync(Rank newRank)
    {
        await RankManager.SetRankAsync(SteamID, newRank);
    }

    //
    //
    // Stats
    //
    //

    public class PlayerStats
    {
        public uint Kills {get;set;} = 0;
        public uint Deaths {get;set;} = 0;
        public uint Fish {get;set;} = 0;
        public uint ItemsLooted {get;set;} = 0;
        public ulong PlayTime {get;set;} = 0;
    }

    private PlayerStats CachedStats => _PlayerState.CachedStats;

    /// <summary>Stats for from logon to logoff</summary>
    public PlayerState.Session ServerSession 
    {
        get => _PlayerState.ServerSession;
        private set => _PlayerState.ServerSession = value;
    }
    /// <summary>Stats for current life</summary>
    public PlayerState.Session LifeSession 
    {
        get => _PlayerState.LifeSession;
        private set => _PlayerState.LifeSession = value;
    }

    public PlayerStats Stats => new() 
    {
        Kills = CachedStats.Kills + ServerSession.Kills,
        Deaths = CachedStats.Deaths + ServerSession.Deaths,
        Fish = CachedStats.Fish + ServerSession.Fish,
        ItemsLooted = CachedStats.ItemsLooted + ServerSession.ItemsLooted,
        PlayTime = 0
    };

    private async UniTask SetCacheStats()
    {
        _PlayerState.CachedStats = await StatsManager.GetStats(SteamID) ?? new();
    }

    private void OnFishCaught(MeowPlayer catcher)
    {
        if (catcher == this)
        {
            AddFish();
        }
    }

    private void OnPlayerKilled(MeowPlayer victim, MeowPlayer killer)
    {
        if (victim == this)
        {
            AddDeath();
            return;
        }

        if (killer == this)
        {
            AddKill();
            return;
        }
    }

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

    private float LastKillTime 
    {
        get => _PlayerState.LastKillTime;
        set => _PlayerState.LastKillTime = value;
    }
    private uint KillStreakCount
    {
        get => _PlayerState.KillStreakCount;
        set => _PlayerState.KillStreakCount = value;
    }
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
            MeowChat.BroadcastMessage(KillStreak, Name, KillStreakCount);
        }
    }

    public async UniTask CommitStatsAsync()
    {
        await StatsManager.CommitSession(SteamID, ServerSession);
    }

    public async UniTask<PlayerStats?> GetStatsAsync()
    {
        return await StatsManager.GetStats(SteamID);
    }

    //
    //
    // Connection
    //
    //

    public void Reconnect()
    {
        MeowPlayerManager.Relog(this);
    }

    public void SendRelayToServer(uint ip, ushort port)
    {
        MeowPlayerManager.SendRelayToServer(this, ip, port);
    }

    //
    //
    // Input
    //
    //

    private float _LastCommand
    {
        get => _PlayerState.LastCommand;
        set => _PlayerState.LastCommand = value;
    }

    public void OnPluginKeyPressed(byte key)
    {
        if (Time.time - _LastCommand < 0.5f)
        {
            return;
        }

        if (!SaveData.CustomBinds.TryGetValue(key, out string value))
        {
            return;
        }

        _LastCommand = Time.time;
        CommandManager.ExecuteCommand(value, this).Forget();
    }

    public void SetBind(byte number, string command)
    {
        number--;
        SaveData.CustomBinds.AddOrUpdate(number, command);
    }

    public bool RemoveBind(byte number)
    {
        number--;
        return SaveData.CustomBinds.Remove(number);
    }
}

