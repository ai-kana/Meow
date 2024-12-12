#pragma once

#include <concord/types.h>

extern struct rcon_request** rcon_requests;
extern size_t rcon_requests_count;

struct rcon_request {
    u64snowflake id;
    char* text;
};

void rcon_enqueue(char* cmd, u64snowflake id);
