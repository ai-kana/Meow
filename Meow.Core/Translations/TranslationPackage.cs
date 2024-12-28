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

    public string Translate(string language, params object[] args)
    {
        return _Translation.Translate(language, _Arguments);
    }

    public string TranslateNoColor(string language, params object[] args)
    {
        return _Translation.TranslateNoColor(language, _Arguments);
    }
}
