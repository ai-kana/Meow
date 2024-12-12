using Cysharp.Threading.Tasks;
using MySqlConnector;
using Steamworks;
using Meow.Core.Sql;

namespace Meow.Core.Bot;

internal static class LinkManager
{
    private const string LinkTable = "`Links`";
    private const string LinkSteamId = "`SteamId`";
    private const string LinkDiscordId = "`DiscordId`";
    private const string LinkCode = "`Code`";

    private const string CreateTablesCommand =
    $"""
    CREATE TABLE IF NOT EXISTS {LinkTable}
    (
        {LinkSteamId} BIGINT UNSIGNED,
        {LinkDiscordId} BIGINT UNSIGNED DEFAULT 0,
        {LinkCode} VARCHAR(6),
        PRIMARY KEY ({LinkSteamId})
    )
    """;

    public async static UniTask CreateTables()
    {
        await using MySqlConnection connection = SqlManager.CreateConnection();
        await connection.OpenAsync();
        await using MySqlCommand command = new(CreateTablesCommand, connection);
        await command.ExecuteNonQueryAsync();
    }

    private const string GetLinkCommand =
    $"""
    SELECT {LinkCode} FROM {LinkTable} WHERE {LinkSteamId}=@{LinkSteamId}
    """;
    private async static UniTask<string?> GetLinkAsync(CSteamID steamId)
    {
        await using MySqlConnection connection = SqlManager.CreateConnection();
        await connection.OpenAsync();

        await using MySqlCommand command = new(GetLinkCommand, connection);
        command.Parameters.Add($"@{LinkSteamId}", MySqlDbType.UInt64).Value = steamId.m_SteamID;

        await using MySqlDataReader reader = await command.ExecuteReaderAsync();

        return await reader.ReadAsync() ? (string)reader[0] : null;
    }

    private const string AddLinkCommand =
    $"""
    INSERT INTO {LinkTable} 
    ({LinkSteamId}, {LinkCode})
    VALUES (@{LinkSteamId}, @{LinkCode})
    """;
    private async static UniTask<string> AddLinkAsync(CSteamID steamId)
    {
        await using MySqlConnection connection = SqlManager.CreateConnection();
        await connection.OpenAsync();

        string code = Guid.NewGuid().ToString().Substring(0, 6);
        await using MySqlCommand command = new(AddLinkCommand, connection);
        command.Parameters.Add($"@{LinkSteamId}", MySqlDbType.UInt64).Value = steamId.m_SteamID;
        command.Parameters.Add($"@{LinkCode}", MySqlDbType.VarChar).Value = code;

        await command.ExecuteNonQueryAsync();

        return code;
    }

    public async static UniTask<string> GetCodeAsync(CSteamID steamId)
    {
        string? code = await GetLinkAsync(steamId);
        if (code == null)
        {
            return await AddLinkAsync(steamId);
        }

        return code;
    }
}
