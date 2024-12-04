using Newtonsoft.Json;

namespace Kronstadt.Core.Roles;

[Serializable]
public class Role
{
    [JsonProperty]
    public string Id {get; private set;} = "";
    [JsonProperty]
    public string ChatTag {get; private set;} = "";
    [JsonProperty]
    public bool DutyOnly {get; private set;} = false;
    [JsonProperty]
    public HashSet<string> Permissions {get; private set;} = null!;
}
