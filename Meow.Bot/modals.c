#include "modals.h"
#include "tickets.h"

#include <string.h>

void handle_modal(struct discord* client, const struct discord_interaction* event) {
    if (strcmp(event->data->custom_id, ID_REPORT_MODAL) == 0) {
        create_report_ticket(client, event);
        return;
    }
    if (strcmp(event->data->custom_id, ID_HELP_MODAL) == 0) {
        create_help_ticket(client, event);
        return;
    }
    if (strcmp(event->data->custom_id, ID_APPEAL_MODAL) == 0) {
        create_appeal_ticket(client, event);
        return;
    }
}
