using SDG.Unturned;

namespace Kronstadt.Core.Players.Components;

public class KronstadtPlayerSkills
{
    public readonly KronstadtPlayer Owner; 
    private PlayerSkills _Skills => Owner.Player.skills; 

    public KronstadtPlayerSkills(KronstadtPlayer owner)
    {
        Owner = owner;
    }

    public uint Experience => _Skills.experience;

    public void GiveExperience(uint xp)
    {
        _Skills.ServerSetExperience(_Skills.experience + xp);
    }

    public void RemoveExperience(uint xp)
    {
        _Skills.ServerSetExperience(_Skills.experience - xp);
    }

    public void SetExperience(uint xp)
    {
        _Skills.ServerSetExperience(xp);
    }
}
