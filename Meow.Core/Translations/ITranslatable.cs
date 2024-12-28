using Meow.Core.Players;

namespace Meow.Core.Translations;

public interface ITranslatable
{
    public string Translate(params object[] args);
    public string Translate(string language, params object[] args);
    public string Translate(IPlayer player, params object[] args);

    public string TranslateNoColor(params object[] args);
    public string TranslateNoColor(string language, params object[] args);
    public string TranslateNoColor(IPlayer player, params object[] args);
}
