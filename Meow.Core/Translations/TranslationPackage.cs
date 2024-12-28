using Meow.Core.Players;

namespace Meow.Core.Translations;

public class TranslationPackage : ITranslatable
{
    private readonly Translation _Translation;
    private readonly object[] _Arguments;

    public TranslationPackage(Translation translation, params object[] args)
    {
        _Translation = translation;
        _Arguments = args;
    }

    public string Translate(string language)
    {
        return _Translation.TranslateNoColor(language, _Arguments);
    }

    public string Translate(params object[] args)
    {
        return _Translation.Translate(_Arguments);
    }

    public string Translate(string language, params object[] args)
    {
        return _Translation.Translate(language, _Arguments);
    }

    public string Translate(IPlayer player, params object[] args)
    {
        return _Translation.Translate(player, _Arguments);
    }

    public string TranslateNoColor(params object[] args)
    {
        return _Translation.TranslateNoColor(_Arguments);
    }

    public string TranslateNoColor(string language, params object[] args)
    {
        return _Translation.TranslateNoColor(language, _Arguments);
    }

    public string TranslateNoColor(IPlayer player, params object[] args)
    {
        return _Translation.TranslateNoColor(player, _Arguments);
    }
}
