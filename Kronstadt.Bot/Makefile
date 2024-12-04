CC = cc
CFLAGS = -Wall -Wextra -g -std=c99
LFLAGS = -pthread -lcurl -I/usr/local/mysql/include -L/usr/local/mysql/lib -lmysqlclient
FFLAGS = -DCCORD_SIGINTCATCH 

OBJDIR := obj
SRCS := $(wildcard *.c)
OBJS := $(patsubst %.c, $(OBJDIR)/%.o, $(SRCS))

OUT = bot

$(OUT): $(OBJS)
ifeq (, $(wildcard ./concord))
	git clone https://github.com/ai-kana/concord.git --depth 1
	make -C ./concord
endif

	$(CC) $(FFLAGS) $(LFLAGS) $(CFLAGS) -o $@ $^ concord/lib/libdiscord.a

$(OBJDIR)/%.o: %.c | $(OBJDIR)
	$(CC) $(CFLAGS) -c -o $@ $< 

clean:
	rm -rf $(OBJDIR)
	rm $(OUT)

debug:
	gdb $(OUT)

.PHONY: clean debug
