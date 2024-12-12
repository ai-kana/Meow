#include "rcon.h"
#include <stdlib.h>
#include <string.h>

struct rcon_request** rcon_requests = NULL;
size_t rcon_requests_count = 0;

static struct rcon_request* alloc_request(char* cmd, u64snowflake id) {
    size_t length = strlen(cmd) + 1;
    size_t size = sizeof(struct rcon_request) + length;
    struct rcon_request* req = malloc(size);
    memset(req, 0, size);

    req->id = id;

    req->text = (char*)(req + 1);
    memcpy(req->text, cmd, length);

    return req;
}

void rcon_enqueue(char* cmd, u64snowflake id) {
    rcon_requests_count++;
    rcon_requests = realloc(rcon_requests, rcon_requests_count * sizeof(void*));
    rcon_requests[rcon_requests_count - 1] = alloc_request(cmd, id);;
}
