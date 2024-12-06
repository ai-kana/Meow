#include "stats_command.h"
#include "sql.h" 
#include <concord/types.h>
#include <stdlib.h>
#include <string.h>
#include <strings.h>

static u64snowflake
get_linked_steam_id(u64snowflake discordId) {
    u64snowflake ret = 0;
    MYSQL* conn = get_conn();

    MYSQL_STMT* statement = mysql_stmt_init(conn);
    if (!statement) {
        goto fail;
    }
    const char* query = "SELECT SteamId FROM Links WHERE DiscordId = ?";
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

    MYSQL_BIND bind_result;
    bzero(&bind_result, sizeof(MYSQL_BIND));
    bind_result.buffer_type = MYSQL_TYPE_LONGLONG;
    bind_result.buffer = &ret;

    if (mysql_stmt_bind_result(statement, &bind_result) != 0) {
        puts("Failed to bind result");
        goto fail_and_clean_stmt;
    }

    if (mysql_stmt_fetch(statement) != 0) {
        puts("Failed to fetch result");
        goto fail_and_clean_stmt;
    }

fail_and_clean_stmt:
    mysql_stmt_close(statement);
fail:
    mysql_close(conn);

    return ret;
}

struct player_stats {
    unsigned int items_found;
    unsigned int fish_caught;
    unsigned int player_kills;
    unsigned int player_deaths;
    long play_time;
};

static bool
get_stats(u64snowflake steamId, struct player_stats* out) {
    bool ret = false;
    MYSQL* conn = get_conn();

    MYSQL_STMT* statement = mysql_stmt_init(conn);
    if (!statement) {
        goto fail;
    }
    const char* query = 
    "SELECT ItemsFound, FishCaught, PlayerKills, PlayerDeaths, PlayTime FROM Stats WHERE SteamId = ?";
    if (mysql_stmt_prepare(statement, query, strlen(query)) != 0) {
        puts("Failed to prep statement");
        goto fail_and_clean_stmt;
    }

    MYSQL_BIND* bind = &(MYSQL_BIND) {};
    bzero(bind, sizeof(MYSQL_BIND));

    printf("%lu binding steamId\n", steamId);
    bind->buffer = &steamId;
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

    MYSQL_BIND bind_result[5] = {0};

    bind_result[0].buffer_type = MYSQL_TYPE_LONG;
    bind_result[0].buffer = &out->items_found;
    bind_result[1].buffer_type = MYSQL_TYPE_LONG;
    bind_result[1].buffer = &out->fish_caught;
    bind_result[2].buffer_type = MYSQL_TYPE_LONG;
    bind_result[2].buffer = &out->player_kills;
    bind_result[3].buffer_type = MYSQL_TYPE_LONG;
    bind_result[3].buffer = &out->player_deaths;
    bind_result[4].buffer_type = MYSQL_TYPE_LONGLONG;
    bind_result[4].buffer = &out->play_time;

    if (mysql_stmt_bind_result(statement, bind_result) != 0) {
        puts("Failed to bind result");
        goto fail_and_clean_stmt;
    }

    if (mysql_stmt_fetch(statement) != 0) {
        puts("Failed to fetch result");
        goto fail_and_clean_stmt;
    }

    printf("Stats: %d, %d, %d, %d, %ld\n", out->items_found, out->fish_caught, out->player_kills, out->player_deaths, out->play_time);
    ret = true;

fail_and_clean_stmt:
    mysql_stmt_close(statement);
fail:
    mysql_close(conn);

    return ret;
}

static void 
respond(struct discord *client, const struct discord_interaction *event, char* message) {
    struct discord_interaction_response test = {
        .type = DISCORD_INTERACTION_CHANNEL_MESSAGE_WITH_SOURCE,
        .data = &(struct discord_interaction_callback_data) {
            .content = message,
            .flags = 64
        }
    };
    
    discord_create_interaction_response(client, event->id, event->token, &test, NULL);
}

void
command_stats(struct discord* client, const struct discord_interaction* event) {
    struct discord_application_command_interaction_data_options* options = event->data->options;
    unsigned long steam_id = 0;
    u64snowflake discord_id = event->member->user->id;
    if (options == NULL) {
        steam_id = get_linked_steam_id(discord_id);
        if (steam_id == 0) {
            respond(client, event, "Your account is not linked");
            return;
        }
    } else {
        char* _;
        if (strcmp(options->array->name, "steamid") == 0) {
            steam_id = strtoul(options->array->value, &_, 10);
            discord_id = 0;
        } else if (strcmp(options->array->name, "discord") == 0) {
            discord_id = strtoul(options->array->value, &_, 10);
            steam_id = get_linked_steam_id(discord_id);
            if (steam_id == 0) {
                respond(client, event, "User does not have their account linked");
                return;
            }
        }
    }

    struct player_stats stats;
    bool state = get_stats(steam_id, &stats);
    if (!state) {
        respond(client, event, "Failed to get stats");
        return;
    }

    struct discord_embed embed = {0};
    discord_embed_set_title(&embed, "Stats");
    if (discord_id == 0) {
        discord_embed_set_description(&embed, "Stats for %lu", steam_id);
    } else {
        discord_embed_set_description(&embed, "Stats for <@%lu>", discord_id);
    }

    char buf[64];
    bzero(buf, 64);

    snprintf(buf, 64, "%d", stats.fish_caught);
    discord_embed_add_field(&embed, "Fish caught", buf, false);

    snprintf(buf, 64, "%d", stats.items_found);
    discord_embed_add_field(&embed, "Items found", buf, false);

    snprintf(buf, 64, "%d", stats.player_kills);
    discord_embed_add_field(&embed, "Kills", buf, false);

    snprintf(buf, 64, "%d", stats.player_deaths);
    discord_embed_add_field(&embed, "Deaths", buf, false);

    const float kd = (float)stats.player_kills / (float)(stats.player_deaths == 0 ? 1 : stats.player_deaths);
    snprintf(buf, 64, "%.2f", kd);
    discord_embed_add_field(&embed, "K/D", buf, false);

    const float seconds_hour = 3600;
    const float play_time_hours = (float)stats.play_time / seconds_hour;

    snprintf(buf, 64, "%.2f hours", play_time_hours);
    discord_embed_add_field(&embed, "Playtime", buf, false);

    struct discord_interaction_response response = {
        .type = DISCORD_INTERACTION_CHANNEL_MESSAGE_WITH_SOURCE,
        .data = &(struct discord_interaction_callback_data) {
            .content = NULL,
            .embeds = &(struct discord_embeds) {
                .size = 1,
                .array = &embed,
            },
        }
    };

    discord_create_interaction_response(client, event->id, event->token, &response, NULL);

    discord_embed_cleanup(&embed);
}
