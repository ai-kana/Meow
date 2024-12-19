using Cysharp.Threading.Tasks;
using Meow.Core.Json;
using Meow.Core.Manifest;

namespace Meow.Core.Roles;

public class RoleManager
{
    public static HashSet<Role> Roles {get; private set;} = new(0);

    public static async UniTask RegisterRoles()
    {
        const string path = "Roles.json";

        if (!File.Exists(path))
        {
            const string manifestPath = "Meow.Core.Roles.json";
            await ManifestHelper.CopyToFile(manifestPath, path);
        }

        using JsonStreamReader reader = new(File.Open(path, FileMode.Open, FileAccess.Read));
        Roles = await reader.ReadObject<HashSet<Role>>() ?? new();
    }

    public static HashSet<Role> GetRoles(HashSet<string> ids)
    {
        HashSet<Role> roles = new();
        foreach (string id in ids)
        {
            Role? role = Roles.Where(x => string.Compare(x.Id, id, StringComparison.OrdinalIgnoreCase) == 0).FirstOrDefault();
            if (role == null)
            {
                continue;
            }

            roles.Add(role);
        }

        return roles;
    }

    public static Role? GetRole(string id)
    {
        Role? role = Roles.Where(x => string.Compare(x.Id, id, StringComparison.OrdinalIgnoreCase) == 0).FirstOrDefault();
        if (role == null)
        {
            return null;
        }

        return role;
    }
}
