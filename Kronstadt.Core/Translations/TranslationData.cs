using Newtonsoft.Json;

namespace Kronstadt.Core.Translations;

[Serializable]
public class TranslationData
{
    [JsonIgnore]
    public string Path {get; set;} = string.Empty;
    [JsonProperty]
    public string? LanguageTitle {get; private set;}
    [JsonProperty]
    public Dictionary<string, string> Translations {get; private set;} = new();
}
