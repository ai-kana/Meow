namespace Kronstadt.Core.Players.Components;

public class KronstadtPlayerConnection
{
    public readonly KronstadtPlayer Owner;
    public KronstadtPlayerConnection(KronstadtPlayer owner)
    {
        Owner = owner;
    }

    public void Reconnect()
    {
        KronstadtPlayerManager.Relog(Owner);
    }

    public void SendRelayToServer(uint ip, ushort port)
    {
        KronstadtPlayerManager.SendRelayToServer(Owner, ip, port);
    }
}
