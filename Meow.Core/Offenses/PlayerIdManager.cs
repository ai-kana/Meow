using Cysharp.Threading.Tasks;
using MySqlConnector;
using Steamworks;
using Meow.Core.Players;
using Meow.Core.Sql;

namespace Meow.Core.Offenses;

public class PlayerIdManager
{
    private struct PlayerIdData
    {
        public string HwidOne;
        public string HwidTwo;
        public uint IPAddress;
    }

    private const string PlayerTable = "Players";
    private const string PlayerSteamID = "SteamID";
    private const string PlayerHwidOne = "HwidOne";
    private const string PlayerHwidTwo = "HwidTwo";
    private const string PlayerIp = "PlayerIp";

    private const string CreateTablesCommand = 
    $"""
    CREATE TABLE IF NOT EXISTS {PlayerTable} (
    {PlayerSteamID} BIGINT UNSIGNED,
    {PlayerHwidOne} VARCHAR(40),
    {PlayerHwidTwo} VARCHAR(40),
    {PlayerIp} INT UNSIGNED
    );
    """;

    public async static UniTask CreateTables()
    { 
        await using MySqlConnection connection = SqlManager.CreateConnection();
        await connection.OpenAsync();
        await using MySqlCommand command = new(CreateTablesCommand, connection);
        await command.ExecuteNonQueryAsync();
    }

    private const string GetHwidsAndIPsCommand = 
    $"""
    SELECT {PlayerHwidOne}, {PlayerHwidTwo}, {PlayerIp} FROM {PlayerTable} WHERE {PlayerSteamID}=@{PlayerSteamID}
    """;
    public static async UniTask<IEnumerable<Offense>> GetOffensesAsync(MeowPlayer player)
    {
        await using MySqlConnection connection = SqlManager.CreateConnection();
        await connection.OpenAsync();

        PlayerIdData current = GetPlayerIdData(player);

        List<PlayerIdData> data = new();
        data.Add(current);
        await using (MySqlCommand command = new(GetHwidsAndIPsCommand, connection))
        {
            command.Parameters.Add($"@{PlayerSteamID}", MySqlDbType.UInt64).Value = player.SteamID.m_SteamID;

            await using (MySqlDataReader reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    PlayerIdData newData = new();
                    newData.HwidOne = (string)reader[0];
                    newData.HwidTwo = (string)reader[1];
                    newData.IPAddress = (uint)reader[2];
                    data.Add(newData);
                }
            }
        }

        HashSet<string> hwidOnes = new(data.Select(x => x.HwidOne));
        HashSet<string> hwidTwos = new(data.Select(x => x.HwidTwo));
        HashSet<uint> ips = new(data.Select(x => x.IPAddress));

        HashSet<CSteamID> foundIds = new();

        bool foundHwidOne = false;
        foreach (string hwid in hwidOnes)
        foreach (CSteamID id in await GetSteamIDsFromHwidOne(hwid, connection))
        {
            foundIds.Add(id);
            foundHwidOne = true;
        }

        bool foundHwidTwo = false;
        foreach (string hwid in hwidTwos)
        foreach (CSteamID id in await GetSteamIDsFromHwidOne(hwid, connection))
        {
            foundIds.Add(id);
            foundHwidTwo = true;
        }

        bool foundIp = false;
        foreach (uint ip in ips)
        foreach (CSteamID id in await GetSteamIDsFromIp(ip, connection))
        {
            foundIds.Add(id);
            foundIp = true;
        }

        if (!(foundHwidOne || foundHwidTwo || foundIp) || !foundIds.Contains(player.SteamID))
        {
            await AddPlayer(player, current, connection);
        }

        HashSet<Offense> offenses = new();
        foreach (CSteamID id in foundIds)
        foreach (Offense offense in await OffenseManager.GetOffenses(id))
        {
            offenses.Add(offense);
        }

        return offenses;
    }
    
    private const string GetIDsFromHwidOneCommand = $"SELECT {PlayerSteamID} FROM {PlayerTable} WHERE {PlayerHwidOne}=@{PlayerHwidOne}";
    private const string GetIDsFromHwidTwoCommand = $"SELECT {PlayerSteamID} FROM {PlayerTable} WHERE {PlayerHwidTwo}=@{PlayerHwidTwo}";
    private const string GetIDsFromIpCommand = $"SELECT {PlayerSteamID} FROM {PlayerTable} WHERE {PlayerIp}=@{PlayerIp} AND {PlayerIp}!=0";
    private static async UniTask<IEnumerable<CSteamID>> GetSteamIDsFromHwidOne(string hwid, MySqlConnection connection)
    {
        List<CSteamID> steamIDs = new();
        await using MySqlCommand command = new(GetIDsFromHwidOneCommand, connection);
        command.Parameters.Add($"@{PlayerHwidOne}", MySqlDbType.VarChar).Value = hwid;

        await using MySqlDataReader reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            steamIDs.Add(new((ulong)reader[0]));
        }

        return steamIDs;
    }

    private static async UniTask<IEnumerable<CSteamID>> GetSteamIDsFromHwidTwo(string hwid, MySqlConnection connection)
    {
        List<CSteamID> steamIDs = new();
        await using MySqlCommand command = new(GetIDsFromHwidTwoCommand, connection);
        command.Parameters.Add($"@{PlayerHwidTwo}", MySqlDbType.VarChar).Value = hwid;

        await using MySqlDataReader reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            steamIDs.Add(new((ulong)reader[0]));
        }

        return steamIDs;
    }

    private static async UniTask<IEnumerable<CSteamID>> GetSteamIDsFromIp(uint ip, MySqlConnection connection)
    {
        List<CSteamID> steamIDs = new();
        await using MySqlCommand command = new(GetIDsFromIpCommand, connection);
        command.Parameters.Add($"@{PlayerIp}", MySqlDbType.UInt32).Value = ip;

        await using MySqlDataReader reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            steamIDs.Add(new((ulong)reader[0]));
        }

        return steamIDs;
    }

    private static PlayerIdData GetPlayerIdData(MeowPlayer player)
    {
        List<byte[]> hwids = player.SteamPlayer.playerID.GetHwids().ToList();
        byte[] hwidOneArray = hwids[0];
        byte[] hwidTwoArray = hwids[1];

        string hwidOne = BitConverter.ToString(hwidOneArray).Replace("-", "");
        string hwidTwo = BitConverter.ToString(hwidTwoArray).Replace("-", "");
        uint ip = player.SteamPlayer.getIPv4AddressOrZero();

        return new()
        {
            HwidOne = hwidOne,
            HwidTwo = hwidTwo,
            IPAddress = ip,
        };
    }

    private const string AddPlayerCommand =
    $"""
    INSERT INTO {PlayerTable} ({PlayerSteamID}, {PlayerHwidOne}, {PlayerHwidTwo}, {PlayerIp})
    VALUES (@{PlayerSteamID}, @{PlayerHwidOne}, @{PlayerHwidTwo}, @{PlayerIp})
    """;
    private static async UniTask AddPlayer(MeowPlayer player, PlayerIdData data, MySqlConnection connection)
    {
        await using MySqlCommand command = new(AddPlayerCommand, connection);
        command.Parameters.Add($"@{PlayerSteamID}", MySqlDbType.UInt64).Value = player.SteamID.m_SteamID;
        command.Parameters.Add($"@{PlayerHwidOne}", MySqlDbType.VarChar).Value = data.HwidOne;
        command.Parameters.Add($"@{PlayerHwidTwo}", MySqlDbType.VarChar).Value = data.HwidTwo;
        command.Parameters.Add($"@{PlayerIp}", MySqlDbType.UInt32).Value = data.IPAddress;
        await command.ExecuteNonQueryAsync();
    }
}
