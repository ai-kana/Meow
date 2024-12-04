#pragma once

#include <concord/discord.h>

void handle_modal(struct discord *client, const struct discord_interaction *event);
