using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Meow.Core.Extensions;
using Meow.Core.Json;

namespace Meow.Core.Translations;

internal class TranslationManager
{
    private const string TranslationsDirectory = "Translations";
    public readonly static HashSet<TranslationData> TranslationData = new();

    static TranslationManager()
    {
        ServerManager.OnServerSave += OnSave;
        Directory.CreateDirectory(TranslationsDirectory);
    }

    private static void OnSave()
    {
        // Needs to be blocking to ensure it saved
        foreach (TranslationData data in TranslationData)
        {
            using StreamWriter writer = new(File.Open(data.Path, FileMode.Create, FileAccess.Write));
            string content = JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented);
            writer.Write(content);
        }
    }

    public static async UniTask LoadTranslations()
    {
        IEnumerable<string> dirs = Directory.GetFiles(TranslationsDirectory, "*.json");
        foreach (string path in dirs)
        {
            using JsonStreamReader reader = new(File.Open(path, FileMode.Open, FileAccess.Read));
            TranslationData data = await reader.ReadObject<TranslationData>() ?? throw new("Failed to load translation");
            if (data.LanguageTitle == null)
            {
                continue;
            }

            data.Path = path;
            TranslationData.Add(data);
        }
    }

    public static bool TryGetTranslation(string language, string key, out string value)
    {
        value = string.Empty;
        TranslationData data = TranslationData.FirstOrDefault(x => x.LanguageTitle == language);
        if (data == null)
        {
            return false;
        }

        return data.Translations.TryGetValue(key, out value);
    }

    public static void AddTranslation(string key, string value)
    {
        foreach (TranslationData data in TranslationData)
        {
            data.Translations.TryAdd(key, value);
        }
    }
}
