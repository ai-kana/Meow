#include "buttons.h"
#include "ticket_command.h"
#include "tickets.h"

#include <string.h>

void handle_button(struct discord* client, const struct discord_interaction *event) {
    if (strcmp(event->data->custom_id, TICKET_ID_HELP) == 0) {
        send_help_ticket_modal(client, event);
        return;
    }

    if (strcmp(event->data->custom_id, TICKET_ID_APPEAL) == 0) {
        send_appeal_ticket_modal(client, event);
        return;
    }

    if (strcmp(event->data->custom_id, TICKET_ID_REPORT) == 0) {
        send_report_ticket_modal(client, event);
        return;
    }

    if (strcmp(event->data->custom_id, ID_CLOSE_BUTTON) == 0) {
        close_button_pressed(client, event);
        return;
    }
}
