using System.Runtime.CompilerServices;

namespace Meow.Core.Bot;

internal class PacketBuilder
{
    private List<byte> _Packet = null!;

    public PacketBuilder()
    {
        Init(0);
    }

    public PacketBuilder(int size)
    {
        Init(size);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Init(int size)
    {
        _Packet = new(size + 4);
        for (int i = 0; i < sizeof(int); i++)
        {
            _Packet.Add(0);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe void WriteBytes(byte* v, int vCount)
    {
        _Packet.Capacity += vCount;
        for (int i = 0; i < vCount; i++)
        {
            _Packet.Add(v[i]);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe void WriteUtf8(char* v, int vLen)
    {
        WriteBytes((byte*)&vLen, sizeof(int));
        _Packet.Capacity += vLen;

        byte* b = (byte*)v;
        const int stepSize = sizeof(char);
        for (int i = 0; i < vLen * stepSize; i += stepSize)
        {
            _Packet.Add(b[i]);
        }
    }

    public unsafe byte[] Build()
    {
        int size = _Packet.Count + 4;
        byte* bytes = (byte*)&size;
        _Packet[0] = bytes[0];
        _Packet[1] = bytes[1];
        _Packet[2] = bytes[2];
        _Packet[3] = bytes[3];
        
        return _Packet.ToArray();
    }

    public void WriteByte(byte b)
    {
        _Packet.Add(b);
    }

    public unsafe void WriteInt32(int v)
    {
        WriteBytes((byte*)&v, sizeof(int));
    }

    public unsafe void WriteUInt64(ulong v)
    {
        WriteBytes((byte*)&v, sizeof(ulong));
    }

    public unsafe void WriteString(string message)
    {
        fixed (char* bPtr = message)
        {
            WriteUtf8(bPtr, message.Length);
        }
    }
}
