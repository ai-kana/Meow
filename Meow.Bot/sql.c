#include "sql.h"
#include "main.h"
#include <concord/discord.h>
#include <string.h>

MYSQL* get_conn() {
    struct ccord_szbuf_readonly buf_server = discord_config_get_field(discord_client, (char *[2]){ "sql", "server" }, 2);
    struct ccord_szbuf_readonly buf_database = discord_config_get_field(discord_client, (char *[2]){ "sql", "database" }, 2);
    struct ccord_szbuf_readonly buf_username = discord_config_get_field(discord_client, (char *[2]){ "sql", "username" }, 2);
    struct ccord_szbuf_readonly buf_password = discord_config_get_field(discord_client, (char *[2]){ "sql", "password" }, 2);

    char server[buf_server.size + 1];
    char database[buf_database.size + 1];
    char username[buf_username.size + 1];
    char password[buf_password.size + 1];

    server[buf_server.size] = 0;
    database[buf_database.size] = 0;
    username[buf_username.size] = 0;
    password[buf_password.size] = 0;

    memcpy(server, buf_server.start, buf_server.size);
    memcpy(database, buf_database.start, buf_database.size);
    memcpy(username, buf_username.start, buf_username.size);
    memcpy(password, buf_password.start, buf_password.size);

    MYSQL* conn = mysql_init(NULL);
    if (conn == NULL)
    {
        return NULL;
    }

    if (mysql_real_connect(conn, server, username, password, database, 0, NULL, 0) == NULL) {
        return NULL;
    }

    return conn;
}
