#include "rcon_command.h"
#include <string.h>
#include "rcon.h"

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

void command_rcon(struct discord *client, const struct discord_interaction *event) {
    struct discord_application_command_interaction_data_options* options = event->data->options;
    if (options->size < 1)
    {
        respond(client, event, "rcon requires one argument");
        return;
    }

    char* command = options->array[0].value;
    rcon_enqueue(command, event->member->user->id);
    respond(client, event, "Command executing... Reply could take up to 10 seconds");
}
