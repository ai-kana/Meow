using Meow.Core.Players;
using Meow.Core.Formatting;

namespace Meow.Core.Translations;

public readonly struct Translation
{
    private readonly string _Key;

    public Translation(string key)
    {
        _Key = key;
        if (!TranslationManager.TryGetTranslation("English", _Key, out _))
        {
            throw new KeyNotFoundException($"Failed to find key: {_Key}");
        }
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

        if (!TranslationManager.TryGetTranslation(language, _Key, out string value))
        {
            throw new();
        }

        return Formatter.Format(value, fixedArgs);
    }

    public string TranslateNoColor(string language, params object[] args)
    {
        string[] fixedArgs = GetTranslatedArguments(language, args);

        if (!TranslationManager.TryGetTranslation(language, _Key, out string value))
        {
            throw new();
        }

        return Formatter.FormatNoColor(value, fixedArgs);
    }

    public TranslationPackage AsPackage(params object[] args)
    {
        return new(this, args);
    }
}
