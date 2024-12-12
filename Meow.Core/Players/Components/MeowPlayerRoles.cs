namespace Meow.Core.Players.Components;

public class MeowPlayerRoles
{
    public HashSet<string> Roles => Owner.SaveData.Roles;

    public readonly MeowPlayer Owner; 

    public MeowPlayerRoles(MeowPlayer owner)
    {
        Owner = owner;
    }

    public bool AddRole(string id)
    {
        return Roles.Add(id);
    }

    public bool RemoveRole(string id)
    {
        return Roles.Remove(id);
    }

    public bool HasRole(string id)
    {
        return Roles.Contains(id);
    }
}
