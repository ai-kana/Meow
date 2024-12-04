using System.Text;

namespace Kronstadt.Core.Bot;

internal class PacketBuilder
{
    private List<byte> _Packet; 
    private int _Size = 0;

    public PacketBuilder()
    {
        _Packet = new(4);
        _Packet.AddRange([0,0,0,0]);
    }

    public PacketBuilder(int size)
    {
        _Packet = new(size + 4);
        _Packet.AddRange([0,0,0,0]);
    }

    private void IncrementHeader(int jump = 1)
    {
        _Size += jump;
    }

    public byte[] Build()
    {
        byte[] bytes = BitConverter.GetBytes(_Size);
        _Packet[0] = bytes[0];
        _Packet[1] = bytes[1];
        _Packet[2] = bytes[2];
        _Packet[3] = bytes[3];

        return _Packet.ToArray();
    }

    public void WriteByte(byte b)
    {
        IncrementHeader();
        _Packet.Add(b);
    }

    public void WriteInt32(int i)
    {
        IncrementHeader(4);
        byte[] bytes = BitConverter.GetBytes(i);
        _Packet.AddRange(bytes);
    }

    public void WriteUInt64(ulong i)
    {
        IncrementHeader(8);
        byte[] bytes = BitConverter.GetBytes(i);
        _Packet.AddRange(bytes);
    }

    public unsafe void WriteString(string message)
    {
        IncrementHeader(message.Length);
        WriteInt32(message.Length);
        _Packet.AddRange(Encoding.ASCII.GetBytes(message));
    }
}
