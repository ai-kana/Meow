namespace Meow.Core.Players.Components;

public class MeowPlayerConnection
{
    public readonly MeowPlayer Owner;
    public MeowPlayerConnection(MeowPlayer owner)
    {
        Owner = owner;
    }

    public void Reconnect()
    {
        MeowPlayerManager.Relog(Owner);
    }

    public void SendRelayToServer(uint ip, ushort port)
    {
        MeowPlayerManager.SendRelayToServer(Owner, ip, port);
    }
}
