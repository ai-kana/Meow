using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Kronstadt.Core.Extensions;
using Kronstadt.Core.Logging;

namespace Kronstadt.Core.Translations;

public class TranslationManager
{
    private static readonly ILogger _Logger;
    private const string TranslationsDirectory = "Translations";
    public readonly static HashSet<TranslationData> TranslationData = new();

    static TranslationManager()
    {
        _Logger = LoggerProvider.CreateLogger<TranslationManager>();
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

    private static async UniTask<TranslationData?> LoadFile(string path)
    {
        using StreamReader reader = new(File.Open(path, FileMode.Open, FileAccess.Read));
        string content = await reader.ReadToEndAsync();
        return JsonConvert.DeserializeObject<TranslationData>(content);
    }

    public static async UniTask LoadTranslations()
    {
        IEnumerable<string> dirs = Directory.GetFiles(TranslationsDirectory, "*.json");
        foreach (string path in dirs)
        {
            TranslationData? data = await LoadFile(path);
            if (data == null)
            {
                continue;
            }

            if (data.LanguageTitle == null)
            {
                continue;
            }

            data.Path = path;
            TranslationData.Add(data);
            _Logger.LogInformation($"Loaded translation for {data.LanguageTitle}");
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
