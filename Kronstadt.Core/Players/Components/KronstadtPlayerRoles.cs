namespace Kronstadt.Core.Players.Components;

public class KronstadtPlayerRoles
{
    public HashSet<string> Roles => Owner.SaveData.Roles;

    public readonly KronstadtPlayer Owner; 

    public KronstadtPlayerRoles(KronstadtPlayer owner)
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
