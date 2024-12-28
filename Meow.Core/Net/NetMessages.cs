using System.Reflection;
using Meow.Core.Logging;
using Meow.Core.Players;
using SDG.NetPak;
using SDG.NetTransport;
using SDG.Unturned;

namespace Meow.Core.Net;

public delegate void ClientWriteHandler(NetPakWriter writer);

public class NetMessages
{
    private static readonly ILogger _Logger;
    static NetMessages()
    {
        _Logger = LoggerProvider.CreateLogger<NetMessages>();

        _NetMessagesType = 
            typeof(Provider).Assembly.GetType("NetMessages") 
            ?? typeof(Provider).Assembly.GetTypes().First(x => x.Name == "NetMessages")
            ?? throw new("Failed to get net messages type");

        _SendMessageMethod = 
            _NetMessagesType.GetMethod("SendMessageToClient", BindingFlags.Static | BindingFlags.Public) 
            ?? throw new("Failed to find Method");

        _ClientWriteHandler = 
            _NetMessagesType.GetNestedType("ClientWriteHandler", BindingFlags.Public)
            ?? throw new("Failed to get client write handler type");

        _NetPakWriter = 
            _NetMessagesType.GetField("writer", BindingFlags.Static | BindingFlags.NonPublic)
            ?? throw new("Failed to get netpak writer");
    }

    private static readonly FieldInfo _NetPakWriter;
    public static NetPakWriter NetMessageWriter => (NetPakWriter)_NetPakWriter.GetValue(null);

    private readonly static Type _NetMessagesType;

    //(EClientMessage index, ENetReliability reliability, ITransportConnection transportConnection, ClientWriteHandler callback)
    private readonly static MethodInfo _SendMessageMethod;

    //delegate(NetPakWriter writer)
    private readonly static Type _ClientWriteHandler;

    public static void SendMessageToClient(MeowPlayer player, EClientMessage message, ENetReliability reliability, Action<NetPakWriter> info)
    {
        NetPakWriter writer = NetMessageWriter;
        writer.Reset();
        writer.WriteEnum(message);

        info(writer);

        writer.Flush();
        player.TransportConnection.Send(writer.buffer, writer.writeByteIndex, ENetReliability.Reliable);
    }
}
