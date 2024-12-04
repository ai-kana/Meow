#include "tickets.h"

#include <concord/channel.h> 
#include <concord/discord_codecs.h> 
#include <concord/interaction.h>
#include <concord/json-build.h>

#include <pthread.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <strings.h>
#include <time.h>

struct ticket_data {
    u64snowflake user_id;
    char* ticket_title;
    int item_count;
    struct item_data {
        char* title;
        char* body;
    }* items;
};

static struct ticket_data* pending_tickets[8];

static struct ticket_data* get_data(u64snowflake id) {
    for (int i = 0; i < 8; i++) {
        if (pending_tickets[i] == NULL) {
            continue;
        }

        if (pending_tickets[i]->user_id == id) {
            struct ticket_data* ret = pending_tickets[i];
            pending_tickets[i] = NULL;
            return ret;
        }
    }

    return NULL;
}

static void add_data(struct ticket_data* data) {
    for (int i = 0; i < 8; i++) {
        if (pending_tickets[i] == NULL) {
            pending_tickets[i] = data;
        }
    }
}

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

static void on_channel_created(struct discord* client, struct discord_response* response, const struct discord_channel* channel) {
    u64snowflake id = channel->permission_overwrites->array[0].id;

    struct ticket_data* data = get_data(id);
    // shouldn't ever happen
    if (data == NULL) {
        return;
    }

    struct discord_embed embed = {0};
    discord_embed_set_title(&embed, data->ticket_title);

    for (int i = 0; i < data->item_count; i++) {
        discord_embed_add_field(&embed, data->items[i].title, data->items[i].body, false);
    }

    struct discord_component close_row;
    struct discord_component close_button;

    init_button(&close_button, "Close", ID_CLOSE_BUTTON);
    init_row(&close_row, &(struct discord_components) {.array = &close_button, .size = 1});

    char content[30];
    snprintf(content, 30, "<@%lu>", id);

    struct discord_create_message message = {
        .content = content,
        .embeds = &(struct discord_embeds) {
            .array = &embed,
            .size = 1,
        },
        .components = &(struct discord_components) {
            .array = &close_row,
            .size = 1,
        }
    };

    discord_create_message(client, channel->id, &message, NULL);

    // Long shot free my favorite
    free(data);
}

#define MEMBER 1
#define ROLE 0

static void create_channel(struct discord *client, const struct discord_interaction *event, char* name) {
    struct discord_overwrites overwrites = {0};
    discord_overwrite_append(&overwrites, event->member->user->id, MEMBER, 1024, 0);
    discord_overwrite_append(&overwrites, event->guild_id, ROLE, 0, 1024);
    // Staff team role id
    discord_overwrite_append(&overwrites, 1207557381668732979, ROLE, 1024, 0);

    struct discord_create_guild_channel channel = {
        .name = name,
        .type = DISCORD_CHANNEL_GUILD_TEXT,
        .parent_id = TICKET_CATEGORY,
        .permission_overwrites = &overwrites
    };

    struct discord_ret_channel ret = {0};
    ret.done = &on_channel_created;
    discord_create_guild_channel(client, event->guild_id, &channel, &ret);

    free(overwrites.array);

    struct discord_interaction_response response = {
        .type = DISCORD_INTERACTION_CHANNEL_MESSAGE_WITH_SOURCE,
        .data = &(struct discord_interaction_callback_data) {   
            .content = "Your ticket has been created.",
            .flags = 64,
        }
    };

    discord_create_interaction_response(client, event->id, event->token, &response, NULL);
}

static void init_input(struct discord_component* input, char* name, char* id, enum discord_component_styles style) {
    bzero(input, sizeof(struct discord_component));
    input->type = DISCORD_COMPONENT_TEXT_INPUT;

    input->style = style;
    input->custom_id = id;
    input->label = name;
    input->required = true;
}

#define INPUT_STEAMID "report_input_steamid"
#define INPUT_REASON "report_input_reason"
void send_help_ticket_modal(struct discord *client, const struct discord_interaction *event) {
    struct discord_component components[1];

    struct discord_component* player_field = &components[0];

    init_input(player_field, "Issue", INPUT_STEAMID, DISCORD_TEXT_PARAGRAPH);
    player_field->min_length = 1;
    player_field->max_length = 250;
    player_field->placeholder = "Describe your issue in detail";

    struct discord_component modal[1] = {0};
    modal[0].type = DISCORD_COMPONENT_ACTION_ROW;
    modal[0].components = &(struct discord_components) {
        .array = &components[0],
        .size = 1
    };

    struct discord_interaction_response response = {
        .type = DISCORD_INTERACTION_MODAL,
        .data = &(struct discord_interaction_callback_data) {
            .title = "Submit help ticket",
            .custom_id = ID_HELP_MODAL,
            .components = &(struct discord_components) {
                .array = modal,
                .size = 1,
            }
        },
    };

    discord_create_interaction_response(client, event->id, event->token, &response, NULL);
}

void send_appeal_ticket_modal(struct discord *client, const struct discord_interaction *event) {
    struct discord_component components[2];

    struct discord_component* player_field = &components[0];
    struct discord_component* reason_field = &components[1];

    init_input(player_field, "SteamID64", INPUT_STEAMID, DISCORD_TEXT_SHORT);
    player_field->min_length = 1;
    player_field->max_length = 20;
    player_field->placeholder = "76561198364938298";

    init_input(reason_field, "Reason for unban", INPUT_REASON, DISCORD_TEXT_PARAGRAPH);
    reason_field->min_length = 1;
    reason_field->max_length = 100;

    struct discord_component modal[2] = {0};
    modal[0].type = DISCORD_COMPONENT_ACTION_ROW;
    modal[0].components = &(struct discord_components) {
        .array = &components[0],
        .size = 1
    };
    modal[1].type = DISCORD_COMPONENT_ACTION_ROW;
    modal[1].components = &(struct discord_components) {
        .array = &components[1],
        .size = 1
    };

    struct discord_interaction_response response = {
        .type = DISCORD_INTERACTION_MODAL,
        .data = &(struct discord_interaction_callback_data) {
            .title = "Appeal your ban",
            .custom_id = ID_APPEAL_MODAL,
            .components = &(struct discord_components) {
                .array = modal,
                .size = 2,
            }
        },
    };

    discord_create_interaction_response(client, event->id, event->token, &response, NULL);
}

void send_report_ticket_modal(struct discord *client, const struct discord_interaction *event) {
    struct discord_component components[2];

    struct discord_component* player_field = &components[0];
    struct discord_component* reason_field = &components[1];

    init_input(player_field, "SteamID64/In-game name", INPUT_STEAMID, DISCORD_TEXT_SHORT);
    player_field->min_length = 1;
    player_field->max_length = 20;
    player_field->placeholder = "76561198364938298";

    init_input(reason_field, "Reason", INPUT_REASON, DISCORD_TEXT_PARAGRAPH);
    reason_field->min_length = 1;
    reason_field->max_length = 100;

    struct discord_component modal[2] = {0};
    modal[0].type = DISCORD_COMPONENT_ACTION_ROW;
    modal[0].components = &(struct discord_components) {
        .array = &components[0],
        .size = 1
    };
    modal[1].type = DISCORD_COMPONENT_ACTION_ROW;
    modal[1].components = &(struct discord_components) {
        .array = &components[1],
        .size = 1
    };

    struct discord_interaction_response response = {
        .type = DISCORD_INTERACTION_MODAL,
        .data = &(struct discord_interaction_callback_data) {
            .title = "Report a player",
            .custom_id = ID_REPORT_MODAL,
            .components = &(struct discord_components) {
                .array = modal,
                .size = 2,
            }
        },
    };

    discord_create_interaction_response(client, event->id, event->token, &response, NULL);
}

static void get_value_index(struct discord_component* comp, size_t* length, size_t* index) {
    char buffer[10000];
    discord_component_to_json(buffer, 10000, comp);

    jsmn_parser parser;
    jsmn_init(&parser);
    jsmntok_t* tokens;
    unsigned int token_count = 0;
    jsmn_parse_auto(&parser, buffer, 1000, &tokens, &token_count);

    jsmnf_loader loader;
    jsmnf_init(&loader);
    jsmnf_pair* pairs;
    unsigned int pair_count = 0;
    jsmnf_load_auto(&loader, buffer, tokens, token_count, &pairs, &pair_count);

    jsmnf_pair* pair = jsmnf_find(pairs, buffer, "value", 5);
    *length = pair->v.len;
    *index = pair->v.pos;

    free(tokens);
    free(pairs);
}

void create_help_ticket(struct discord *client, const struct discord_interaction *event) {
    struct discord_component* name_component = &event->data->components->array[0].components->array[0];

    size_t issue_length;
    size_t issue_index;
    get_value_index(name_component, &issue_length, &issue_index);

    // Add remove for '\0'
    issue_length++;

    const char* issue_field = "Issue";
    const size_t issue_field_length = strlen(issue_field) + 1;

    const char* title = "Help";
    const size_t title_length = strlen(title) + 1;

    const size_t str_length = issue_field_length + issue_length + title_length;
    const size_t length = sizeof(struct ticket_data) + (sizeof(struct item_data)) + str_length;

    struct ticket_data* data = malloc(length);
    bzero(data, length);
    data->item_count = 1;
    data->user_id = event->member->user->id;

    struct item_data* items = (struct item_data*)(data + 1);
    //items->title = (char*)(items + 2);
    items->title = (char*)(items + 1);
    items->body = items->title + issue_field_length;

    data->ticket_title = items->body + issue_length;

    memcpy(data->ticket_title, title, title_length - 1);
    memcpy(items->title, issue_field, issue_field_length);
    memcpy(items->body, name_component->value, issue_length - 1);

    data->items = items;

    add_data(data);

    char buffer[42] = {0};
    sprintf(buffer, "%s-%s", title, event->member->user->username);

    create_channel(client, event, buffer);
}

void create_appeal_ticket(struct discord *client, const struct discord_interaction *event) {
    struct discord_component* name_component = &event->data->components->array[0].components->array[0];
    struct discord_component* description_component = &event->data->components->array[1].components->array[0];

    size_t name_length;
    size_t name_index;
    get_value_index(name_component, &name_length, &name_index);

    size_t description_length;
    size_t description_index;
    get_value_index(description_component, &description_length, &description_index);
    
    // Add remove for '\0'
    name_length++;
    description_length++;

    const char* name_field = "SteamID64";
    const size_t name_field_length = strlen(name_field) + 1;
    const char* description_field = "Reason for unban";
    const size_t description_field_length = strlen(description_field) + 1;

    const char* title = "Appeal";
    const size_t title_length = strlen(title) + 1;

    const size_t str_length = name_field_length + description_field_length + description_length + name_length + title_length;
    const size_t length = sizeof(struct ticket_data) + (sizeof(struct item_data) * 2) + str_length;

    struct ticket_data* data = malloc(length);
    memset(data, '\0', length);
    data->item_count = 2;
    data->user_id = event->member->user->id;

    struct item_data* items = (struct item_data*)(data + 1);
    items[0].title = (char*)(items + 2);
    items[0].body = items[0].title + name_field_length;
    items[1].title = items[0].body + name_length;
    items[1].body = items[1].title + description_field_length;

    data->ticket_title = items[1].body + description_field_length;
    memcpy(data->ticket_title, title, title_length - 1);

    memcpy(items[0].title, name_field, name_field_length);
    memcpy(items[1].title, description_field, description_field_length);

    memcpy(items[0].body, name_component->value, name_length - 1);
    memcpy(items[1].body, description_component->value, description_length - 1);

    data->items = items;

    add_data(data);

    char buffer[42] = {0};
    sprintf(buffer, "%s-%s", title, event->member->user->username);

    create_channel(client, event, buffer);
}

void create_report_ticket(struct discord *client, const struct discord_interaction *event) {
    struct discord_component* name_component = &event->data->components->array[0].components->array[0];
    struct discord_component* description_component = &event->data->components->array[1].components->array[0];

    size_t name_length;
    size_t name_index;
    get_value_index(name_component, &name_length, &name_index);

    size_t description_length;
    size_t description_index;
    get_value_index(description_component, &description_length, &description_index);
    
    // Add remove for '\0'
    name_length++;
    description_length++;

    const char* name_field = "Player";
    const size_t name_field_length = strlen(name_field) + 1;
    const char* description_field = "Description";
    const size_t description_field_length = strlen(description_field) + 1;

    const char* title = "Report";
    const size_t title_length = strlen(title) + 1;

    const size_t str_length = name_field_length + description_field_length + description_length + name_length + title_length;
    const size_t length = sizeof(struct ticket_data) + (sizeof(struct item_data) * 2) + str_length;

    struct ticket_data* data = malloc(length);
    memset(data, '\0', length);
    data->item_count = 2;
    data->user_id = event->member->user->id;

    struct item_data* items = (struct item_data*)(data + 1);
    items[0].title = (char*)(items + 2);
    items[0].body = items[0].title + name_field_length;
    items[1].title = items[0].body + name_length;
    items[1].body = items[1].title + description_field_length;

    data->ticket_title = items[1].body + description_field_length;
    memcpy(data->ticket_title, title, title_length - 1);

    memcpy(items[0].title, name_field, name_field_length);
    memcpy(items[1].title, description_field, description_field_length);

    memcpy(items[0].body, name_component->value, name_length - 1);
    memcpy(items[1].body, description_component->value, description_length - 1);

    data->items = items;

    add_data(data);

    char buffer[42] = {0};
    sprintf(buffer, "%s-%s", title, event->member->user->username);

    create_channel(client, event, buffer);
}

#define TICKET_LOG_ID 1285810871372611584
void close_button_pressed(struct discord *client, const struct discord_interaction *event) {
    struct discord_delete_channel delete = {
        .reason = "Ticket closed"
    };

    discord_delete_channel(client, event->channel_id, &delete, NULL);

    char file_name[50];
    bzero(file_name, 50);
    sprintf(file_name, "%lu.txt", event->channel_id);

    FILE* file = fopen(file_name, "w+");
    if (file == NULL) {
        fclose(file);
        return;
    }
    fclose(file);

    const char* ticket_header = "Ticket log of `%lu`";
    const size_t length = strlen(ticket_header) - 2 + 50;
    char buffer[length];
    bzero(buffer, length);

    sprintf(buffer, ticket_header, event->member->user->id);
    
    struct discord_attachment log_file = {
        .filename = file_name,
    };

    struct discord_create_message message = {
        .content = buffer,
        .attachments = &(struct discord_attachments) {
            .array = &log_file,
            .size = 1
        }
    };

    struct discord_ret_message ret = {0};
    struct discord_message msg;
    ret.sync = &msg;
    discord_create_message(client, TICKET_LOG_ID, &message, &ret);
    
    remove(file_name);
}

pthread_mutex_t lock;
void archive_message(struct discord *client, const struct discord_message *event) {
    if (event->author->bot) {
        return;
    }

    struct discord_channel channel;
    struct discord_ret_channel ret = {0};
    ret.sync = &channel;

    discord_get_channel(client, event->channel_id, &ret);
    if (channel.parent_id != TICKET_CATEGORY) {
        return;
    }

    const size_t name_len = strlen(event->author->username);
    const size_t msg_len = strlen(event->content);

    char file_name[50] = {0};
    sprintf(file_name, "%lu.txt", channel.id);

    FILE* file = fopen(file_name, "a+");
    fwrite(event->author->username, 1, name_len, file);
    fwrite(": ", 1, 2, file);
    fwrite(event->content, 1, msg_len, file);
    fputc('\n', file);

    fflush(file);
    fclose(file);
}
