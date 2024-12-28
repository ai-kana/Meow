using Meow.Core.Players;

namespace Meow.Core.Translations;

public interface ITranslatable
{
    public string Translate(string language, params object[] args);
    public string TranslateNoColor(string language, params object[] args);
}

public static class ITranslatableExtensions
{
    public static string Translate(this ITranslatable instance, params object[] args)
    {
        return instance.Translate("English", args);
    }

    public static string Translate(this ITranslatable instance, IPlayer player, params object[] args)
    {
        return instance.Translate(player.Language, args);
    }

    public static string TranslateNoColor(this ITranslatable instance, params object[] args)
    {
        return instance.TranslateNoColor("English", args);
    }

    public static string TranslateNoColor(this ITranslatable instance, IPlayer player, params object[] args)
    {
        return instance.TranslateNoColor(player.Language, args);
    }
}
