using Meow.Core.Configuration;
using Meow.Core.Startup;
using Microsoft.Extensions.Configuration;
using MySqlConnector;

namespace Meow.Core.Sql;

[Startup]
public class SqlManager
{
    private static string Server = null!;
    private static string Database = null!;
    private static string UserID = null!;
    private static string Password = null!;

    static SqlManager()
    {
        ConfigurationEvents.OnConfigurationReloaded += OnReloaded;
    }

    private static void OnReloaded()
    {
        IConfigurationSection section = MeowHost.Configuration.GetSection("Sql");
        Server = section.GetValue<string>("Server") ?? throw new("Failed to get sql: Server");
        Database = section.GetValue<string>("Database") ?? throw new("Failed to get sql: Database");
        UserID = section.GetValue<string>("UserID") ?? throw new("Failed to get sql: UserID");
        Password = section.GetValue<string>("Password") ?? throw new("Failed to get sql: Password");
    }

    public static MySqlConnection CreateConnection()
    {
        MySqlConnectionStringBuilder builder = new() 
        {
            Server = Server,
            Database = Database,
            UserID = UserID,
            Password = Password 
        };

        return new(builder.ConnectionString);
    }
}
