using Cysharp.Threading.Tasks;
using Kronstadt.Core.Players;
using Kronstadt.Core.Players.Components;
using Kronstadt.Core.Sql;
using MySqlConnector;
using Steamworks;
using Session = Kronstadt.Core.Players.Components.KronstadtPlayerStats.Session;

namespace Kronstadt.Core.Stats;

public class StatsManager
{
    private const string StatsTable = "`Stats`";
    private const string SteamId = "`SteamId`";
    private const string ItemsFound = "`ItemsFound`";
    private const string FishCaught = "`FishCaught`";
    private const string PlayerKills = "`PlayerKills`";
    private const string PlayerDeaths = "`PlayerDeaths`";
    private const string PlayTime = "`PlayTime`";

    private const string CreateTablesCommand =
    $"""
    CREATE TABLE IF NOT EXISTS {StatsTable} (
        {SteamId} BIGINT UNSIGNED,
        {ItemsFound} INT UNSIGNED DEFAULT 0,
        {FishCaught} INT UNSIGNED DEFAULT 0,
        {PlayerKills} INT UNSIGNED DEFAULT 0,
        {PlayerDeaths} INT UNSIGNED DEFAULT 0,
        {PlayTime} BIGINT UNSIGNED DEFAULT 0,
        PRIMARY KEY ({SteamId})
    )
    """;

    static StatsManager()
    {
        KronstadtPlayerManager.OnPlayerConnected += OnConnected;
        KronstadtPlayerManager.OnPlayerDisconnected += OnDisconnected;
    }

    private static void OnConnected(KronstadtPlayer player)
    {
        _ = CreatePlayerEntry(player);
    }

    private static void OnDisconnected(KronstadtPlayer player)
    {
        _ = player.Stats.CommitStatsAsync();
    }

    public static async UniTask CreateTables()
    {
        await using MySqlConnection connection = SqlManager.CreateConnection();
        await connection.OpenAsync();

        await using MySqlCommand command = new(CreateTablesCommand, connection);
        await command.ExecuteNonQueryAsync();
    }

    private const string CreatePlayerEntryCommand =
    $"""
    INSERT IGNORE INTO {StatsTable} ({SteamId}) VALUES (@SteamId)
    """;
    public static async UniTask CreatePlayerEntry(KronstadtPlayer player)
    {
        await using MySqlConnection connection = SqlManager.CreateConnection();
        await connection.OpenAsync();

        await using MySqlCommand command = new(CreatePlayerEntryCommand, connection);
        command.Parameters.Add("@SteamId", MySqlDbType.UInt64).Value = player.SteamID.m_SteamID;

        await command.ExecuteNonQueryAsync();
    }

    private const string CommitSessionCommand =
    $"""
    UPDATE {StatsTable}
    SET 
        {FishCaught}={FishCaught} + @FishCaught,
        {PlayerDeaths}={PlayerDeaths} + @PlayerDeaths,
        {PlayerKills}={PlayerKills} + @PlayerKills,
        {ItemsFound}={ItemsFound} + @ItemsFound,
        {PlayTime}={PlayTime} + @PlayTime
    WHERE {SteamId}=@SteamId
    """;
    public static async UniTask CommitSession(KronstadtPlayer owner, Session session)
    {
        await using MySqlConnection connection = SqlManager.CreateConnection();
        await connection.OpenAsync();

        await using MySqlCommand command = new(CommitSessionCommand, connection);

        ulong realTime = (ulong)(DateTimeOffset.Now.ToUnixTimeSeconds() - session.StartTime);
        command.Parameters.Add("@SteamId", MySqlDbType.UInt64).Value = owner.SteamID.m_SteamID;
        command.Parameters.Add("@FishCaught", MySqlDbType.UInt32).Value = session.Fish;
        command.Parameters.Add("@PlayerDeaths", MySqlDbType.UInt32).Value = session.Deaths; 
        command.Parameters.Add("@PlayerKills", MySqlDbType.UInt32).Value = session.Kills;
        command.Parameters.Add("@ItemsFound", MySqlDbType.UInt32).Value = session.ItemsLooted;
        command.Parameters.Add("@PlayTime", MySqlDbType.UInt64).Value = realTime;

        await command.ExecuteNonQueryAsync();
    }

    private const string GetStatsCommand =
    $"""
    SELECT {FishCaught}, {PlayerDeaths}, {PlayerKills}, {ItemsFound}, {PlayTime}
    FROM {StatsTable}
    WHERE {SteamId}=@SteamId
    """;
    public static async UniTask<PlayerStats?> GetStats(CSteamID steamId)
    {
        await using MySqlConnection connection = SqlManager.CreateConnection();
        await connection.OpenAsync();

        await using MySqlCommand command = new(GetStatsCommand, connection);
        command.Parameters.Add("@SteamId", MySqlDbType.UInt32).Value = steamId.m_SteamID;

        await using MySqlDataReader reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            return null;
        }

        return new() 
        {
            Fish = (uint)reader[0],
            Deaths = (uint)reader[1],
            Kills = (uint)reader[2],
            ItemsLooted = (uint)reader[3],
            PlayTime = (ulong)reader[4]
        };
    }
}
