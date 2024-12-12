using SDG.Unturned;

namespace Meow.Core.Players.Components;

public class MeowPlayerSkills
{
    public readonly MeowPlayer Owner; 
    private PlayerSkills _Skills => Owner.Player.skills; 

    public MeowPlayerSkills(MeowPlayer owner)
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
