using Cysharp.Threading.Tasks;
using Meow.Core.Commands.Framework;
using Meow.Core.Extensions;
using Meow.Core.Formatting;
using Meow.Core.Players;
using Meow.Core.Translations;

namespace Meow.Core.Commands;

[CommandData("language", "lang")]
[CommandSyntax("[<Params: language> <Switches: list>]")]
internal class LanguageCommand : Command
{
    public LanguageCommand(CommandContext context) : base(context)
    {
    }

    private static readonly Translation LanguageNotFound = new("LanguageNotFound", "{0} is not a supported language");
    private static readonly Translation LanguageSet = new("LanguageSet", "Set your language to {0}");

    public override UniTask ExecuteAsync()
    {
        Context.AssertPlayer(out MeowPlayer caller);

        string arg = Context.Current;

        TranslationData data = TranslationManager.TranslationData
            .FirstOrDefault(x => x.LanguageTitle!.Contains(arg, StringComparison.InvariantCultureIgnoreCase));
        if (data == null)
        {
            throw Context.Reply(LanguageNotFound, arg);
        }

        string language = data.LanguageTitle!;
        caller.SaveData.Language = language;

        throw Context.Reply(LanguageSet, caller.Language);
    }
}

[CommandData("list")]
[CommandParent(typeof(LanguageCommand))]
internal class LanguageListCommand : Command
{
    public LanguageListCommand(CommandContext context) : base(context)
    {
    }

    private static readonly Translation LanguageList = new("LanguageList", "List of available languages: {0}");

    public override UniTask ExecuteAsync()
    {
        Context.AssertPlayer();

        IEnumerable<string> languages = 
            TranslationManager.TranslationData
            .Select(x => x.LanguageTitle)
            .Where(x => x != null)
            .Cast<string>();

        throw Context.Reply(LanguageList, Formatter.FormatList(languages, ", "));
    }
}
