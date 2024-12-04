#pragma once

#include <concord/discord.h>

#define TICKET_PREFIX ticket

#define ID_REPORT_MODAL "modal_report"
#define ID_APPEAL_MODAL "modal_appeal"
#define ID_HELP_MODAL "modal_help"

#define ID_CLOSE_BUTTON "button_close"

#define TICKET_CATEGORY 1277057173632847892

void close_button_pressed(struct discord *client, const struct discord_interaction *event);

void create_help_ticket(struct discord *client, const struct discord_interaction *event);
void create_appeal_ticket(struct discord *client, const struct discord_interaction *event);
void create_report_ticket(struct discord *client, const struct discord_interaction *event);

void send_help_ticket_modal(struct discord *client, const struct discord_interaction *event);
void send_appeal_ticket_modal(struct discord *client, const struct discord_interaction *event);
void send_report_ticket_modal(struct discord *client, const struct discord_interaction *event);

void archive_message(struct discord *client, const struct discord_message *event);
