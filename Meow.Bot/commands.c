#include "commands.h"
#include "link.h"
#include "stats_command.h"
#include "ticket_command.h"
#include "rcon_command.h"
#include <string.h>

void handle_command(struct discord *client, const struct discord_interaction *event) {
    if (strcmp(event->data->name, "createticket") == 0) {
        command_ticket(client, event);
        return;
    }

    if (strcmp(event->data->name, "link") == 0) {
        link_account(client, event);
        return;
    }

    if (strcmp(event->data->name, "unlink") == 0) {
        unlink_account(client, event);
        return;
    }

    if (strcmp(event->data->name, "rcon") == 0) {
        command_rcon(client, event);
        return;
    }

    if (strcmp(event->data->name, "stats") == 0) {
        command_stats(client, event);
        return;
    }
}
