using Microsoft.Extensions.Configuration;
using MySqlConnector;

namespace Meow.Core.Sql;

public class SqlManager
{
    public static MySqlConnection CreateConnection()
    {
        IConfigurationSection section = MeowHost.Configuration.GetSection("Sql");
        MySqlConnectionStringBuilder builder = new() 
        {
            Server = section.GetValue<string>("Server"),
            Database = section.GetValue<string>("Database"),
            UserID = section.GetValue<string>("UserID"),
            Password = section.GetValue<string>("Password")
        };

        return new(builder.ConnectionString);
    }
}
