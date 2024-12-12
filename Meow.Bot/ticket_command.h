#pragma once

#include <concord/discord.h>

#define TICKET_ID_APPEAL "appeal_ticket"
#define TICKET_ID_REPORT "report_ticket"
#define TICKET_ID_HELP "help_ticket"

void command_ticket(struct discord *client, const struct discord_interaction *event);
