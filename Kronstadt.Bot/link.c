#include "link.h"
#include "main.h"
#include <concord/types.h>
#include <string.h>
#include <mysql/mysql.h>
#include <stdlib.h>
#include <strings.h>

static MYSQL* get_conn() {
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

static bool try_unlink(u64snowflake discordId) {
    long rows = 0;
    MYSQL* conn = get_conn();

    MYSQL_STMT* statement = mysql_stmt_init(conn);
    if (!statement) {
        goto fail;
    }
    const char* query = "UPDATE Links SET DiscordId = 0 WHERE DiscordId = ?";
    if (mysql_stmt_prepare(statement, query, strlen(query)) != 0) {
        puts("Failed to prep statement");
        goto fail_and_clean_stmt;
    }

    MYSQL_BIND* bind = &(MYSQL_BIND) {};
    bzero(bind, sizeof(MYSQL_BIND));

    bind->buffer = &discordId;
    bind->buffer_type = MYSQL_TYPE_LONGLONG;
    bind->is_unsigned = true;
    bind->length = 0;

    if (mysql_stmt_bind_param(statement, bind)) {
        puts("Failed to bind statement");
        goto fail_and_clean_stmt;
    }

    if (mysql_stmt_execute(statement)) {
        puts("Failed to exe statement");
        goto fail_and_clean_stmt;
    }

    rows = mysql_stmt_affected_rows(statement);

fail_and_clean_stmt:
    mysql_stmt_close(statement);
fail:
    mysql_close(conn);

    return rows > 0;
}

static bool try_link(char* code, u64snowflake discordId) {
    long rows = 0;

    MYSQL* conn = get_conn();

    MYSQL_STMT* statement = mysql_stmt_init(conn);
    if (!statement) {
        goto fail;
    }
    const char* query = "UPDATE Links SET DiscordId = ? WHERE Code = ?";
    if (mysql_stmt_prepare(statement, query, strlen(query)) != 0) {
        puts("Failed to prep statement");
        goto fail_and_clean_stmt;
    }

    MYSQL_BIND bind[2];
    bzero(bind, sizeof(MYSQL_BIND) * 2);

    bind[0].buffer = &discordId;
    bind[0].buffer_type = MYSQL_TYPE_LONGLONG;
    bind[0].is_unsigned = true;
    bind[0].length = 0;

    bind[1].buffer = code;
    bind[1].buffer_type = MYSQL_TYPE_STRING;
    size_t len = strlen(code);
    bind[1].buffer_length = len;
    bind[1].length = &len;

    if (mysql_stmt_bind_param(statement, bind)) {
        puts("Failed to bind statement");
        goto fail_and_clean_stmt;
    }

    if (mysql_stmt_execute(statement)) {
        puts("Failed to exe statement");
        goto fail_and_clean_stmt;
    }

    rows = mysql_stmt_affected_rows(statement);

fail_and_clean_stmt:
    mysql_stmt_close(statement);
fail:
    mysql_close(conn);

    return rows > 0;
}

static void respond(struct discord *client, const struct discord_interaction *event, char* message) {
    struct discord_interaction_response test = {
        .type = DISCORD_INTERACTION_CHANNEL_MESSAGE_WITH_SOURCE,
        .data = &(struct discord_interaction_callback_data) {
            .content = message,
            .flags = 64
        }
    };
    
    discord_create_interaction_response(client, event->id, event->token, &test, NULL);
}

void link_account(struct discord *client, const struct discord_interaction *event) {
    struct discord_application_command_interaction_data_options* options = event->data->options;
    if (options->size < 1)
    {
        respond(client, event, "Link requires one argument");
        return;
    }

    char* code = options->array[0].value;
    const int len = strlen(code);
    if (len != 6)
    {
        respond(client, event, "A link code is 6 characters long");
        return;
    }

    bool linked = try_link(code, event->member->user->id);
    if (linked) {
        respond(client, event, "Successfully linked discord account");
        return;
    }

    respond(client, event, "Failed to link discord account");
}

void unlink_account(struct discord *client, const struct discord_interaction *event) {
    u64snowflake id = event->member->user->id;
    bool linked = try_unlink(id);

    if (linked) {
        respond(client, event, "Successfully unlinked discord account");
        return;
    }

    respond(client, event, "Failed to unlink discord account");
}
