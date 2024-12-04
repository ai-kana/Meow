using System.Reflection;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;

namespace Kronstadt.Core.Roles;

public class RoleManager
{
    public static HashSet<Role> Roles {get; private set;} = new(0);

    private static async UniTask CreateFile()
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        const string path = "Kronstadt.Core.Roles.json";
        using StreamReader reader = new(assembly.GetManifestResourceStream(path));

        string content = await reader.ReadToEndAsync();

        using StreamWriter writer = new(File.Create("Roles.json"));
        await writer.WriteAsync(content);
    }

    public static async UniTask RegisterRoles()
    {
        string path = "Roles.json";

        if (!File.Exists(path))
        {
            await CreateFile();
        }

        using StreamReader stream = new(File.Open(path, FileMode.Open, FileAccess.Read));
        string text = await stream.ReadToEndAsync();

        Roles = JsonConvert.DeserializeObject<HashSet<Role>>(text) ?? new();
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
