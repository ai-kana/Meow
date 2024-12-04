using Cysharp.Threading.Tasks;
using MySqlConnector;
using Steamworks;
using Kronstadt.Core.Sql;

namespace Kronstadt.Core.Offenses;

// God i hate sql
// I FUCKING HATE IT
// its better when no headache

// what did you do lmfaoo 
public class OffenseManager
{
    private const string OffenseTable = "Offenses";
    private const string OffenseId = "Id";
    private const string OffenseType = "Type";
    private const string OffenseOffender = "Offender";
    private const string OffenseIssuer = "Issuer";
    private const string OffenseIssued = "Issued";
    private const string OffenseDuration = "Duration";
    private const string OffensePardoned = "Pardoned";
    private const string OffenseReason = "Reason";

    private const string CreateTablesCommand = 
    $"""
    CREATE TABLE IF NOT EXISTS {OffenseTable} (
    {OffenseId} INT NOT NULL AUTO_INCREMENT,
    {OffenseType} TINYINT,
    {OffenseOffender} BIGINT UNSIGNED,
    {OffenseIssuer} BIGINT UNSIGNED,
    {OffenseIssued} BIGINT,
    {OffenseDuration} BIGINT,
    {OffensePardoned} TINYINT,
    {OffenseReason} VARCHAR(100),
    PRIMARY KEY (Id)
    );
    """;

    public async static UniTask CreateTables()
    { 
        await using MySqlConnection connection = SqlManager.CreateConnection();
        await connection.OpenAsync();
        await using MySqlCommand command = new(CreateTablesCommand, connection);
        await command.ExecuteNonQueryAsync();
    }

    private const string GetOffensesCommand = $"SELECT * FROM {OffenseTable} WHERE {OffenseOffender}=@offender";
    private static Offense ReadOffense(MySqlDataReader reader)
    {
        return new() 
        {
            Id = (int)reader[0],
            OffenseType = (OffenseType)(sbyte)reader[1],
            Offender = (ulong)reader[2],
            Issuer = (ulong)reader[3],
            Issued = (long)reader[4],
            Duration = (long)reader[5],
            Pardoned = ((sbyte)reader[6]) == 1 ? true : false,
            Reason = (string)reader[7],
        };
    }

    private static async UniTask<IEnumerable<Offense>> ReadOffenses(MySqlDataReader reader)
    {
        List<Offense> ret = new();
        while (await reader.ReadAsync())
        {
            ret.Add(ReadOffense(reader));
        }

        return ret;
    }

    public static async UniTask<IEnumerable<Offense>> GetOffenses(CSteamID offender)
    {
        await using MySqlConnection connection = SqlManager.CreateConnection();
        await connection.OpenAsync();
        await using MySqlCommand command = new(GetOffensesCommand, connection);
        command.Parameters.Add("@offender", MySqlDbType.UInt64).Value = offender.m_SteamID;
        await using MySqlDataReader reader = await command.ExecuteReaderAsync();

        return await ReadOffenses(reader);
    }

    private const string GetWarnOffensesCommand = $"SELECT * FROM {OffenseTable} WHERE {OffenseOffender}=@offender AND {OffenseType}=2";
    public static async UniTask<IEnumerable<Offense>> GetWarnOffenses(CSteamID offender)
    {
        await using MySqlConnection connection = SqlManager.CreateConnection();
        await connection.OpenAsync();
        await using MySqlCommand command = new(GetWarnOffensesCommand, connection);
        command.Parameters.Add("@offender", MySqlDbType.UInt64).Value = offender.m_SteamID;
        await using MySqlDataReader reader = await command.ExecuteReaderAsync();

        return await ReadOffenses(reader);
    }

    private const string GetMuteOffensesCommand = $"SELECT * FROM {OffenseTable} WHERE {OffenseOffender}=@offender AND {OffenseType}=1";
    public static async UniTask<IEnumerable<Offense>> GetMuteOffenses(CSteamID offender)
    {
        await using MySqlConnection connection = SqlManager.CreateConnection();
        await connection.OpenAsync();
        await using MySqlCommand command = new(GetMuteOffensesCommand, connection);
        command.Parameters.Add("@offender", MySqlDbType.UInt64).Value = offender.m_SteamID;
        await using MySqlDataReader reader = await command.ExecuteReaderAsync();

        return await ReadOffenses(reader);
    }

    private const string GetBanOffensesCommand = $"SELECT * FROM {OffenseTable} WHERE {OffenseOffender}=@offender AND {OffenseType}=0";
    public static async UniTask<IEnumerable<Offense>> GetBanOffenses(CSteamID offender)
    {
        await using MySqlConnection connection = SqlManager.CreateConnection();
        await connection.OpenAsync();
        await using MySqlCommand command = new(GetBanOffensesCommand, connection);
        command.Parameters.Add("@offender", MySqlDbType.UInt64).Value = offender.m_SteamID;
        await using MySqlDataReader reader = await command.ExecuteReaderAsync();

        return await ReadOffenses(reader);
    }

    private const string AddOffenseCommand = 
        $"""
        INSERT INTO {OffenseTable}
        ({OffenseType}, {OffenseOffender}, {OffenseIssuer}, {OffenseIssued}, {OffenseDuration}, {OffensePardoned}, {OffenseReason})
        VALUES (@{OffenseType}, @{OffenseOffender}, @{OffenseIssuer}, @{OffenseIssued}, @{OffenseDuration}, @{OffensePardoned}, @{OffenseReason})
        """;
    public static async UniTask AddOffense(Offense offense)
    {
        await using MySqlConnection connection = SqlManager.CreateConnection();
        await connection.OpenAsync();

        await using MySqlCommand command = new(AddOffenseCommand, connection);
        command.Parameters.Add($"@{OffenseType}", MySqlDbType.Byte).Value = offense.OffenseType;
        command.Parameters.Add($"@{OffenseOffender}", MySqlDbType.UInt64).Value = offense.Offender;
        command.Parameters.Add($"@{OffenseIssuer}", MySqlDbType.UInt64).Value = offense.Issuer;
        command.Parameters.Add($"@{OffenseIssued}", MySqlDbType.Int64).Value = offense.Issued;
        command.Parameters.Add($"@{OffenseDuration}", MySqlDbType.Int64).Value = offense.Duration;
        command.Parameters.Add($"@{OffensePardoned}", MySqlDbType.Byte).Value = offense.Pardoned ? 1 : 0;
        command.Parameters.Add($"@{OffenseReason}", MySqlDbType.VarChar).Value = offense.Reason;

        await command.ExecuteNonQueryAsync();
    }

    private const string PardonOffenseCommand = $"UPDATE {OffenseTable} SET {OffensePardoned}=1 WHERE {OffenseId}=@{OffenseId}";
    public static async UniTask PardonOffense(int id)
    {
        await using MySqlConnection connection = SqlManager.CreateConnection();
        await connection.OpenAsync();
        await using MySqlCommand command = new(PardonOffenseCommand, connection);
        command.Parameters.Add($"@{OffenseId}", MySqlDbType.Int32).Value = id;

        await command.ExecuteNonQueryAsync();
    }
}
