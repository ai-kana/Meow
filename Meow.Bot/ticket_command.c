#include "ticket_command.h"

#include <concord/discord_codecs.h>
#include <concord/log.h>

static void init_button(struct discord_component* button, char* label, char* id) {
    discord_component_init(button);
    button->label = label;
    button->custom_id = id;
    button->type = DISCORD_COMPONENT_BUTTON;
    button->style = DISCORD_BUTTON_PRIMARY;
}

static void init_row(struct discord_component* row, struct discord_components* buttons) {
    discord_component_init(row);
    row->type = DISCORD_COMPONENT_ACTION_ROW;
    row->components = buttons;
}

void command_ticket(struct discord *client, const struct discord_interaction *event) {
    struct discord_component rows[2];
    struct discord_component* ticket_row = rows;
    struct discord_component* appeal_row = &rows[1];

    struct discord_component components[3];
    struct discord_component* help_ticket = components;
    struct discord_component* report_ticket = &components[1];
    struct discord_component* appeal_ticket = &components[2];

    init_button(help_ticket, "Help Ticket", TICKET_ID_HELP);
    init_button(report_ticket, "Report Ticket", TICKET_ID_REPORT);
    init_button(appeal_ticket, "Appeal Ticket", TICKET_ID_APPEAL);

    discord_component_init(appeal_row);

    init_row(ticket_row, &(struct discord_components) {.size = 2, .array = components});
    init_row(appeal_row, &(struct discord_components) {.size = 1, .array = appeal_ticket});

    struct discord_embed embed = {0};
    discord_embed_set_title(&embed, "Tickets");
    discord_embed_set_description(&embed, "Create a ticket");

    struct discord_interaction_response response = {
        .type = DISCORD_INTERACTION_CHANNEL_MESSAGE_WITH_SOURCE,
        .data = &(struct discord_interaction_callback_data) {
            .content = ".",
            .embeds = &(struct discord_embeds) {
                .size = 1,
                .array = &embed,
            },
            .components = &(struct discord_components) {
                .size = 2,
                .array = rows
            }
        }
    };

    discord_create_interaction_response(client, event->id, event->token, &response, NULL);
}
