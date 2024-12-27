using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Configuration;
using Meow.Core.Commands.Framework;
using Meow.Core.Logging;
using Meow.Core.Players;
using Cysharp.Threading.Tasks;

namespace Meow.Core.Bot;

internal readonly struct RconCommandReply
{
    public readonly ulong InteractionId;
    public readonly string Text;
    public RconCommandReply(ulong interactionId, string text)
    {
        InteractionId = interactionId;
        Text = text;
    }
}

internal class BotManager
{
    private static ConcurrentQueue<string> _LogQueue; 

    static ILogger _Logger;

    static BotManager()
    {
        _Logger = LoggerProvider.CreateLogger<BotManager>();
        CommandReplies = new();
        _LogQueue = new();
    }

    public static void EnqueueLog(string message)
    {
        _LogQueue.Enqueue(message);
    }

    public static ConcurrentQueue<RconCommandReply> CommandReplies;

    private static byte[] ConstructPacket()
    {
        PacketBuilder builder = new();

        builder.WriteByte((byte)MeowPlayerManager.Players.Count);
        foreach (MeowPlayer player in MeowPlayerManager.Players)
        {
            builder.WriteString(player.Name);
        }

        builder.WriteByte((byte)CommandReplies.Count);
        while (CommandReplies.TryDequeue(out RconCommandReply reply))
        {
            builder.WriteUInt64(reply.InteractionId);
            builder.WriteString(reply.Text);
        }

        return builder.Build();
    }

    private static void HandleCommands(byte[] packet)
    {
        byte count = packet[0];
        int offset = 1;
        for (int i = 0; i < count; i++)
        {
            ulong id = BitConverter.ToUInt64(packet, offset);
            offset += sizeof(ulong);
            int length = BitConverter.ToInt32(packet, offset);
            offset += sizeof(int);
            string text = Encoding.UTF8.GetString(packet, offset, length);

            CommandManager.ExecuteCommand(text, new RconPlayer(id, text));
        }
    }

    private static async UniTask HandleConnection(TcpClient client)
    {
        using Stream stream = client.GetStream();
        while (client.Connected)
        {
            byte[] packet = ConstructPacket();
            await stream.WriteAsync(packet, 0, packet.Length);

            byte[] lenBuf = new byte[4];
            await stream.ReadAsync(lenBuf, 0, 4);
            int len = BitConverter.ToInt32(lenBuf, 0);

            packet = new byte[len];
            int x = await stream.ReadAsync(packet, 0, len);
            HandleCommands(packet);

            await UniTask.Delay(10 * 1000);
        }
    }

    private static async UniTask TryConnect(string host, int port)
    {
        using TcpClient client = new();
        await client.ConnectAsync(host, port);

        await HandleConnection(client);
    }

    public static async UniTask Start()
    {
        IConfigurationSection bot = MeowHost.Configuration.GetSection("Bot");
        bool enabled = bot.GetValue<bool>("Enabled");
        if (!enabled)
        {
            return;
        }

        string host = bot.GetValue<string>("Host") ?? throw new("Failed to get host");
        int port = bot.GetValue<int>("Port"); 

        while (true)
        {
            try
            {
                await TryConnect(host, port);
            }
            catch
            {
                //_Logger.LogError(exception.ToString());
            }

            await UniTask.Delay(5000);
        }
    }
}
