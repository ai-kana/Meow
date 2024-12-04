namespace Kronstadt.Core.Players;

public class KronstadtPlayerAdministration
{
    private readonly KronstadtPlayer Owner;
    public bool OnDuty {get; private set;} = false;
    public bool GodMode {get; private set;} = false;
    
    public KronstadtPlayerAdministration(KronstadtPlayer owner)
    {
        Owner = owner;
    }

    private bool _VanishMode = false;
    public bool VanishMode
    {
        get => _VanishMode;
        set
        {
            _VanishMode = value;
            Owner.Player.movement.canAddSimulationResultsToUpdates = !_VanishMode;
            Owner.Movement.Teleport(Owner);
        }
    }

    public bool ToggleGod()
    {
        Owner.Life.Heal();
        GodMode = !GodMode;
        return GodMode;
    }

    public bool ToggleDuty()
    {
        OnDuty = !OnDuty;
        GodMode = OnDuty;
        VanishMode = VanishMode || !OnDuty;

        if (Owner.Permissions.HasPermission("spectator"))
        {
            Owner.Player.look.sendFreecamAllowed(OnDuty);
            Owner.Player.look.sendWorkzoneAllowed(OnDuty);
            Owner.Player.look.sendSpecStatsAllowed(OnDuty);
        }

        return OnDuty;
    }
}
