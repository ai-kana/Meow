#include "server.h"
#include "main.h"
#include "rcon.h"

#include <concord/discord.h>

#include <concord/types.h>
#include <string.h>
#include <sys/time.h>
#include <sys/epoll.h>
#include <netinet/in.h>
#include <stdbool.h>
#include <stdio.h>
#include <stdlib.h>
#include <strings.h>
#include <sys/socket.h>
#include <pthread.h>
#include <unistd.h>

static int socket_fd = 0;

static int get_names_size(unsigned char* packet) {
    unsigned char player_count = packet[0];
    int total_size = 0;
    int n = 1;
    for (int i = 0; i < player_count; i++) {
        int* name_size = (int*)(packet + n);
        n += 4;
        n += *name_size;
        total_size += *name_size + 1;
    }

    return total_size;
}

#define STATUS_MESSAGE 1316323247717093377

static void update_status_offline() {
    struct discord_edit_message msg = {
        .content = "The server is offline",
    };
    discord_edit_message(discord_client, 1277075166803001394, STATUS_MESSAGE, &msg, NULL);
}

static int update_status_embed(unsigned char* packet) {
    unsigned char player_count = packet[0];

    int size = get_names_size(packet) + 1;
    char names[size];
    bzero(names, size);
    int names_i = 0;

    int n = 1;
    for (int i = 0; i < player_count; i++) {
        int* name_size = (int*)(packet + n);
        n += 4;

        char* name = (char*)(packet + n);

        memcpy(names + names_i, name, *name_size);
        names_i += (*name_size) + 1;
        names[names_i - 1] = '\n';

        n += *name_size;
    }

    struct discord_embed embed = {0};
    discord_embed_init(&embed);
    discord_embed_set_title(&embed, "Player count %d/200", player_count);
    discord_embed_add_field(&embed, "Players:", names, false);

    struct discord_edit_message msg = {
        .content = "",
        .embeds = &(struct discord_embeds) {
            .array = &embed,
            .size = 1,
        }
    };
    discord_edit_message(discord_client, 1316320149527330886, STATUS_MESSAGE, &msg, NULL);

    /*
    struct discord_create_message msg = {
        .content = "",
        .embeds = &(struct discord_embeds) {
            .array = &embed,
            .size = 1
        }
    };

    discord_create_message(discord_client, 1316320149527330886, &msg, NULL);
    */

    discord_embed_cleanup(&embed);

    return n;
}

#define RCON_CHANNEL 1316321819044610090
static const char* format = "<@%lu>: %s";
static void handle_reply(const u64snowflake discordId, const unsigned int size, const char* msg) {
    char buf[1028] = {0};
    snprintf(buf, 1027, format, discordId, msg);
    struct discord_create_message reply = {
        .content = buf,
    };

    discord_create_message(discord_client, RCON_CHANNEL, &reply, NULL);
}

static int process_replies(unsigned char* packet, int offset) {
    unsigned char cmd_count = packet[offset];
    offset++;

    for (int i = 0; i < cmd_count; i++) {
        unsigned long* id = (unsigned long*)(packet + offset);
        offset += sizeof(unsigned long);

        unsigned int* str_len = (unsigned int*)(packet + offset);
        offset += sizeof(unsigned int);

        char* msg = (char*)(packet + offset);
        offset += *str_len;

        handle_reply(*id, *str_len, msg);
    }

    return offset;
}

static void send_commands(int fd) {
    int length = 0;
    const size_t count = rcon_requests_count;
    struct rcon_request** restrict base = rcon_requests;
    for (int i = 0; i < count; i++) {
        struct rcon_request* req = base[i];
        length += sizeof(u64snowflake /*unsigned long*/);
        length += sizeof(int);
        length += strlen(req->text);
    }

    length++;
    unsigned char packet[length + 4];
    memcpy(packet, &length, sizeof(int));

    size_t offset = sizeof(int);
    packet[offset] = count;
    offset++;
    for (int i = 0; i < count; i++) {
        struct rcon_request* restrict req = base[i];
        memcpy(packet + offset, &req->id, sizeof(u64snowflake));
        offset += sizeof(u64snowflake);

        const int size = (int)strlen(req->text);
        memcpy(packet + offset, &size, sizeof(int));
        offset += sizeof(int);

        memcpy(packet + offset, req->text, size);
        offset += size;

        free(req);
    }
    free(base);
    rcon_requests = NULL;
    rcon_requests_count = 0;

    write(fd, packet, length + 4);
}

static int listen_for_server(int fd) {
    if (listen(fd, 1) != 0) {
        perror("Failed to get connection");
        return -1;
    }

    socklen_t length = sizeof(struct sockaddr_in);
    struct sockaddr_in client;
    bzero(&client, sizeof(struct sockaddr_in));

    update_status_offline();

    int client_fd = accept(fd, (struct sockaddr*)&client, &length);
    if (client_fd < 0) {
        perror("Failed to get client file descriptor");
        return -1;
    }

    int size;
    while (read(client_fd, &size, 4) != 0) {
        unsigned char packet[size];
        if (read(client_fd, packet, size) == 0) return client_fd;

        int offset = update_status_embed(packet);
        process_replies(packet, offset);

        send_commands(client_fd);
    }

    return client_fd;
}

static void* thread_start(void* param) {
    struct sockaddr_in server;
    bzero(&server, sizeof(struct sockaddr_in));

    server.sin_family = AF_INET;
    server.sin_addr.s_addr = htonl(INADDR_ANY);
    server.sin_port = htons(5001);

    socket_fd = socket(AF_INET, SOCK_STREAM, 0);
    if (socket_fd == -1) {
        perror("Failed to create socket");
        exit(0);
    }

    if (bind(socket_fd, (struct sockaddr*)&server, sizeof(struct sockaddr)) != 0){
        perror("Failed to bind socket");
        exit(0);
    }

    while (true) {
        int client = listen_for_server(socket_fd);

        if (client != -1) {
            close(client);
        }
    }

    return NULL;
}

void start_server() {
    pthread_t thread;
    pthread_create(&thread, NULL, &thread_start, NULL);
}

void stop_server() {
    close(socket_fd);
}
