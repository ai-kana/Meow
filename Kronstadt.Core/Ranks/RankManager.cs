using Cysharp.Threading.Tasks;
using MySqlConnector;
using Steamworks;
using Kronstadt.Core.Sql;

namespace Kronstadt.Core.Ranks;

public enum Rank : byte
{
    None,
    Vip,
    VipPlus,
    Mvp,
    MvpPlus,
    Pro,
    ProPlus
}

public static class RankManager
{
    private const string RankTable = "`Ranks`";
    private const string PlayerId = "`SteamId`";
    private const string PlayerRank = "`Rank`";
    private const string CreateTablesCommand =
    $"""
    CREATE TABLE IF NOT EXISTS {RankTable} (
        {PlayerId} BIGINT UNSIGNED,
        {PlayerRank} TINYINT UNSIGNED,
        PRIMARY KEY ({PlayerId})
    )
    """;
    public static async UniTask CreateTables()
    {
        await using MySqlConnection connection = SqlManager.CreateConnection();
        await connection.OpenAsync();

        await using MySqlCommand command = new(CreateTablesCommand, connection);
        await command.ExecuteNonQueryAsync();
    }

    private const string GetRankCommand =
    $"""
    SELECT {PlayerRank} FROM {RankTable} WHERE {PlayerId}=@PlayerId
    """;
    public static async UniTask<Rank> GetRankAsync(CSteamID id)
    {
        await using MySqlConnection connection = SqlManager.CreateConnection();
        await connection.OpenAsync();

        await using MySqlCommand command = new(GetRankCommand, connection);
        command.Parameters.Add("@PlayerId", MySqlDbType.UInt64).Value = id.m_SteamID;
        await using MySqlDataReader reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            return Rank.None;
        }

        return (Rank)reader[0];
    }

    private const string SetRankCommand =
    $"""
    INSERT INTO {RankTable} ({PlayerId}, {PlayerRank}) VALUES (@PlayerId, @PlayerRank)
    ON DUPLICATE KEY
    UPDATE {PlayerRank}=@PlayerRank
    """;
    public static async UniTask SetRankAsync(CSteamID id, Rank newRank)
    {
        await using MySqlConnection connection = SqlManager.CreateConnection();
        await connection.OpenAsync();

        await using MySqlCommand command = new(SetRankCommand, connection);
        command.Parameters.Add("@PlayerId", MySqlDbType.UInt64).Value = id.m_SteamID;
        command.Parameters.Add("@PlayerRank", MySqlDbType.UByte).Value = (byte)newRank;

        await command.ExecuteNonQueryAsync();
    }
}
