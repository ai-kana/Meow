using System.Reflection;
using SDG.NetPak;
using SDG.Unturned;

namespace Meow.Core.Players;

public class MeowPlayerAdministration
{
    private readonly MeowPlayer Owner;
    public bool OnDuty {get; private set;} = false;
    public bool FakedDisconnected {get; private set;} = false;

    private bool _GodMode = false;
    public bool GodMode 
    {
        get => _GodMode; 
        set 
        {
            _GodMode = value;
            if (_GodMode)
            {
                Owner.Life.Heal();
            }
        }
    }
    
    public MeowPlayerAdministration(MeowPlayer owner)
    {
        Owner = owner;
        /*
        foreach (MeowPlayer player in MeowPlayerManager.Players.Values)
        {
            if (player.Administration.VanishMode)
            {
                //ReplicateRemoveClient.Invoke(null, [player]);

                NetMessages.SendMessageToClient(Owner, EClientMessage.PlayerDisconnected, ENetReliability.Reliable, writer => 
                {
                    writer.WriteNetId(player.SteamPlayer.GetNetId());
                });
            }
        }
        */
    }

    private bool _VanishMode = false;
    public bool VanishMode
    {
        get => _VanishMode;
        set
        {
            _VanishMode = value;
            //ToggleVanish();

            Owner.Player.movement.canAddSimulationResultsToUpdates = !_VanishMode;
            Owner.Movement.Teleport(Owner);
        }
    }

    // (SteamPlayer)
    private static MethodInfo ReplicateRemoveClient =
        typeof(Provider).GetMethod("ReplicateRemoveClient", BindingFlags.Static | BindingFlags.NonPublic);

    //(NetPakWriter writer, SteamPlayer aboutPlayer, SteamPlayer forPlayer)
    private static MethodInfo WriteConnectedMessage =
        typeof(Provider).GetMethod("WriteConnectedMessage", BindingFlags.Static | BindingFlags.NonPublic)
        ?? throw new("Failed to get connect method");

    private static void WriteConnected(NetPakWriter writer, SteamPlayer player, SteamPlayer toPlayer)
    {
        WriteConnectedMessage.Invoke(null, [writer, player, toPlayer]);
    }

    private void ToggleVanish()
    {
        if (_VanishMode)
        {
            FakedDisconnected = true;
            _VanishMode = true;

            Provider.onServerDisconnected?.Invoke(Owner.SteamID);
            ReplicateRemoveClient.Invoke(null, [Owner.SteamPlayer]);
        }
        else if (FakedDisconnected)
        {
            Owner.Player.sendRelayToServer(Provider.ip, Provider.port, "", false);

            FakedDisconnected = false;
            _VanishMode = false;
        }
    }

    /*
    foreach (MeowPlayer player in MeowPlayerManager.Players.Values.Where(x => x != Owner))
    {
        NetPakWriter writer = NetMessages.NetMessageWriter;
        writer.Reset();
        writer.WriteEnum(EClientMessage.PlayerConnected);
        WriteConnected(writer, Owner.SteamPlayer, player.SteamPlayer);
        writer.Flush();
        player.SteamPlayer.transportConnection.Send(writer.buffer, writer.writeByteIndex, ENetReliability.Reliable);
    
    foreach (MeowPlayer player in MeowPlayerManager.Players.Values.Where(x => x != Owner))
    {
        SendInitialState.Invoke(player.Player, [Owner.SteamPlayer]);
    
    try
    {
        Provider.onServerConnected.Invoke(Owner.SteamID);
        Provider.onEnemyConnected.Invoke(Owner.SteamPlayer);
    }
    catch 
    {
    }
    */

    public bool ToggleDuty()
    {
        OnDuty = !OnDuty;
        GodMode = OnDuty;
        //VanishMode = false;

        Owner.Movement.SetSpeed(1f);
        Owner.Movement.SetJump(1f);
        Owner.Movement.SetGravity(1f);

        if (Owner.Permissions.HasPermission("spectator"))
        {
            Owner.Player.look.sendFreecamAllowed(OnDuty);
            Owner.Player.look.sendWorkzoneAllowed(OnDuty);
            Owner.Player.look.sendSpecStatsAllowed(OnDuty);
        }

        return OnDuty;
    }
}
