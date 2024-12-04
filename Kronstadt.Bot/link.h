#pragma once

#include <concord/discord.h>

void link_account(struct discord *client, const struct discord_interaction *event);
void unlink_account(struct discord *client, const struct discord_interaction *event);
