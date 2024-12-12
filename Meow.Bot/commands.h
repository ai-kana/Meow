#pragma once

#include <concord/discord.h>

void handle_command(struct discord *client, const struct discord_interaction *event);
