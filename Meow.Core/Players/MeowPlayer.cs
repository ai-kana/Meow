using Cysharp.Threading.Tasks;
using SDG.Unturned;
using Steamworks;
using Meow.Core.Chat;
using Meow.Core.Fishing;
using Meow.Core.Players.Components;
using Meow.Core.Translations;
using SDG.NetTransport;

namespace Meow.Core.Players;

public class MeowPlayer : IPlayer, IFormattable
{
    public SteamPlayer SteamPlayer {get; private set;}
    public Player Player => SteamPlayer.player;
    
    public string Name {get;}
    public string LogName {get;}
    public CSteamID SteamID => SteamPlayer.playerID.steamID;

    public PlayerData SaveData {get; private set;}

    public ITransportConnection TransportConnection => SteamPlayer.transportConnection;

    public readonly MeowPlayerMovement Movement; 
    public readonly MeowPlayerRoles Roles; 
    public readonly MeowPlayerPermissions Permissions; 
    public readonly MeowPlayerLife Life; 
    public readonly MeowPlayerQuests Quests;
    public readonly MeowPlayerCooldowns Cooldowns;
    public readonly MeowPlayerSkills Skills;
    public readonly MeowPlayerInventory Inventory;
    public readonly MeowPlayerClothing Clothing;
    public readonly MeowPlayerModeration Moderation;
    public readonly MeowPlayerAdministration Administration;
    public readonly MeowPlayerRank Rank;
    public readonly MeowPlayerStats Stats;
    public readonly MeowPlayerConnection Connection;

    public FishingSkill FishingSkill => SaveData.Fishing;

    public CSteamID? LastPrivateMessage {get; set;} = null;

    public string Language => SaveData.Language;

    public static async UniTask<MeowPlayer> CreateAsync(SteamPlayer player)
    {
        return new(player, await PlayerDataManager.LoadDataAsync(player.playerID.steamID));
    }

    private MeowPlayer(SteamPlayer player, PlayerData data)
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
        Connection = new(this);
    }

    public override int GetHashCode()
    {
        return SteamID.GetHashCode();
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
}
