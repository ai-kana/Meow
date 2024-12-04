#include "main.h"
#include "commands.h"
#include "buttons.h"
#include "modals.h"
#include "tickets.h"
#include "server.h"

#include <concord/application_command.h>
#include <concord/discord-events.h>
#include <concord/discord-response.h>
#include <concord/discord_codecs.h>
#include <concord/types.h>
#include <stdlib.h>
#include <string.h>
#include <pthread.h>
#include <stdio.h>

struct discord* discord_client;

void* console_thread_start(void* client) {
    char line[64];
    while (true) {
        scanf("%63[^\n]", line);
        if (strcmp(line, "q") == 0) {
            discord_shutdown(client);
            stop_server();
            exit(0);
        }
    }

    return NULL;
}

void on_channel_create(struct discord *client, const struct discord_channel *channel) {
    const size_t count = sizeof("ticket");

    if (strncmp(channel->name, "ticket", count) == 0) {
    }
}

void on_ready(struct discord *client, const struct discord_ready *event) {
    struct discord_ret_application_commands ret = {
        .sync = &(struct discord_application_commands) {0},
    };
    discord_get_guild_application_commands(client, event->application->id, GUILD_ID, &ret);
    for (int i = 0; i < ret.sync->size; i++) {
        discord_delete_guild_application_command(client, event->application->id, GUILD_ID, ret.sync->array[i].id, NULL);
    }

    struct discord_create_guild_application_command create_ticket_command = {
        .name = "createticket",
        .description = "testing",
        .type = 1,
        .default_permission = true,
        .default_member_permissions = 8
    };
    discord_create_guild_application_command(client, event->application->id, GUILD_ID, &create_ticket_command, NULL);

    struct discord_create_guild_application_command unlink_command = {
        .name = "unlink",
        .description = "Remove link between your discord and steam account",
        .type = 1,
        .default_permission = false
    };
    discord_create_guild_application_command(client, event->application->id, GUILD_ID, &unlink_command, NULL);

    struct discord_create_guild_application_command link_command = {
        .name = "link",
        .description = "Links your discord to your steam account",
        .type = 1,
        .default_permission = true,
        .options = &(struct discord_application_command_options) {
            .size = 1,
            .array = &(struct discord_application_command_option) {
                .name = "code",
                .description = "Code provided by the server to link your account",
                .type = DISCORD_APPLICATION_OPTION_STRING,
                .required = true,
                .options = NULL
            }
        }
    };
    discord_create_guild_application_command(client, event->application->id, GUILD_ID, &link_command, NULL);

    struct discord_create_guild_application_command rcon_command = {
        .name = "rcon",
        .description = "Execute command on the server",
        .type = 1,
        .default_permission = true,
        .default_member_permissions = 8,
        .options = &(struct discord_application_command_options) {
            .size = 1,
            .array = &(struct discord_application_command_option) {
                .name = "command",
                .description = "The command the server will execute",
                .type = DISCORD_APPLICATION_OPTION_STRING,
                .required = true,
                .options = NULL
            }
        }
    };
    discord_create_guild_application_command(client, event->application->id, GUILD_ID, &rcon_command, NULL);
}

void on_interaction(struct discord *client, const struct discord_interaction *event) {
    switch (event->type) {
        case DISCORD_INTERACTION_APPLICATION_COMMAND:
            handle_command(client, event);
            return;
        case DISCORD_INTERACTION_MESSAGE_COMPONENT:
            handle_button(client, event);
            return;
        case DISCORD_INTERACTION_MODAL_SUBMIT:
            handle_modal(client, event);
            return;
        default: 
            return;
    }
}

#define donator_role_count 1
const u64snowflake donator_roles[donator_role_count] = {
    1271802581017952318
};

int main() {
    //const char* token = getenv("unturnov_token");
    //struct discord* client = discord_init(token);

    discord_client = discord_config_init("config.json");
    discord_add_intents(discord_client, 
            DISCORD_GATEWAY_MESSAGE_CONTENT 
            | DISCORD_GATEWAY_GUILD_MEMBERS
            | DISCORD_GATEWAY_GUILD_PRESENCES);

    start_server();

    struct ccord_szbuf_readonly buf = discord_config_get_field(discord_client, (char *[2]){ "sql", "username" }, 2);

    pthread_t console_thread;
    pthread_create(&console_thread, NULL, &console_thread_start, discord_client);

    discord_set_on_message_create(discord_client, &archive_message);
    discord_set_on_ready(discord_client, &on_ready);
    discord_set_on_interaction_create(discord_client, &on_interaction);
    discord_set_on_channel_create(discord_client, &on_channel_create);

    discord_run(discord_client);
    discord_cleanup(discord_client);
    ccord_global_cleanup();
}
