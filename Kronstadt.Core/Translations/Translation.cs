using Kronstadt.Core.Players;
using Kronstadt.Core.Formatting;
//using Unturnov.Core.Logging;

namespace Kronstadt.Core.Translations;

public class Translation
{
    private readonly string _DefaultValue;
    private readonly string _Key;

    public Translation(string key, string defaultValue)
    {
        _DefaultValue = defaultValue;
        _Key = key;
        TranslationManager.AddTranslation(_Key, _DefaultValue);
    }

    private string[] GetTranslatedArguments(string language, object[] args)
    {
        string[] outArgs = new string[args.Length];
        for (int i = 0; i < args.Length; i++)
        {
            object arg = args[i];
            
            if (arg is TranslationPackage translation)
            {
                outArgs[i] = translation.Translate(language);
                continue;
            }
            
            outArgs[i] = arg.ToString();
        }

        return outArgs;
    }

    public string Translate(params object[] args)
    {
        return Translate("English", args);
    }

    public string Translate(IPlayer player, params object[] args)
    {
        return Translate(player.Language, args);
    }

    public string Translate(string language, params object[] args)
    {
        string[] fixedArgs = GetTranslatedArguments(language, args);

        if (language == "English")
        {
            return Formatter.Format(_DefaultValue, fixedArgs);
        }

        if (!TranslationManager.TryGetTranslation(language, _Key, out string value))
        {
            return Formatter.Format(_DefaultValue, fixedArgs);
        }

        return Formatter.Format(value, fixedArgs);
    }

    public string TranslateNoColor(string language, params object[] args)
    {
        string[] fixedArgs = GetTranslatedArguments(language, args);

        if (language == "English")
        {
            return Formatter.FormatNoColor(_DefaultValue, fixedArgs);
        }

        if (!TranslationManager.TryGetTranslation(language, _Key, out string value))
        {
            return Formatter.FormatNoColor(_DefaultValue, fixedArgs);
        }

        return Formatter.FormatNoColor(value, fixedArgs);
    }
}
