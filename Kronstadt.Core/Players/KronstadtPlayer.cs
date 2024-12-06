using Cysharp.Threading.Tasks;
using SDG.Unturned;
using Steamworks;
using Kronstadt.Core.Chat;
using Kronstadt.Core.Fishing;
using Kronstadt.Core.Players.Components;
using Kronstadt.Core.Translations;

namespace Kronstadt.Core.Players;

public class KronstadtPlayer : IPlayer, IFormattable
{
    public SteamPlayer SteamPlayer {get; private set;}
    public Player Player => SteamPlayer.player;
    
    public string Name {get;}
    public string LogName {get;}
    public CSteamID SteamID => SteamPlayer.playerID.steamID;

    public PlayerData SaveData {get; private set;}

    public readonly KronstadtPlayerMovement Movement; 
    public readonly KronstadtPlayerRoles Roles; 
    public readonly KronstadtPlayerPermissions Permissions; 
    public readonly KronstadtPlayerLife Life; 
    public readonly KronstadtPlayerQuests Quests;
    public readonly KronstadtPlayerCooldowns Cooldowns;
    public readonly KronstadtPlayerSkills Skills;
    public readonly KronstadtPlayerInventory Inventory;
    public readonly KronstadtPlayerClothing Clothing;
    public readonly KronstadtPlayerModeration Moderation;
    public readonly KronstadtPlayerAdministration Administration;
    public readonly KronstadtPlayerRank Rank;
    public readonly KronstadtPlayerStats Stats;

    public FishingSkill FishingSkill => SaveData.Fishing;

    public CSteamID? LastPrivateMessage {get; set;} = null;

    public string Language => SaveData.Language;

    public static async UniTask<KronstadtPlayer> CreateAsync(SteamPlayer player)
    {
        return new(player, await PlayerDataManager.LoadDataAsync(player.playerID.steamID));
    }

    private KronstadtPlayer(SteamPlayer player, PlayerData data)
    {
        SaveData = data;
        SteamPlayer = player;
        Name = SteamPlayer.playerID.characterName;
        LogName = $"{Name} ({SteamID})";

        SteamPlayer.player.interact.sendSalvageTimeOverride(2f);

        Movement = new(this);
        Roles = new(this);
        Permissions = new(this);
        Life = new(this);
        Quests = new(this);
        Cooldowns = new(this);
        Skills = new(this);
        Inventory = new(this);
        Clothing = new(this);
        Moderation = new(this);
        Administration = new(this);
        Rank = new(this);
        Stats = new(this);
    }

    public string ToString(string format, IFormatProvider formatProvider)
    {
        return Name;
    }

    public void SendMessage(string format, params object[] args)
    {
        KronstadtChat.BroadcastMessage(this, format, args);
    }

    public void SendMessage(Translation translation, params object[] args)
    {
        KronstadtChat.BroadcastMessage(translation.Translate(Language, args));
    }
}
