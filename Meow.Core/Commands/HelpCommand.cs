using System.Reflection;
using Cysharp.Threading.Tasks;
using Meow.Core.Commands.Framework;
using Meow.Core.Translations;

namespace Meow.Core.Commands;

[CommandData("help")]
[CommandSyntax("[<Params: command> <Switch: commands>]")]
internal class HelpCommand : Command
{
    public HelpCommand(CommandContext context) : base(context)
    {
    }

    private readonly Translation HelpFormat = new("HelpFormat");

    public override UniTask ExecuteAsync()
    {
        Context.AssertArguments(1);

        string command = Context.Form();
        CommandTokenizer tokenizer = new(command);
        IEnumerable<string> tokens = tokenizer.Parse();

        Type? type = CommandManager.GetCommandType(tokens);
        if (type == null)
        {
            throw Context.Reply("There is no command called {0}", command);
        }

        CommandSyntaxAttribute? syntax = type.GetCustomAttribute<CommandSyntaxAttribute>();
        string content = "none";
        if (syntax != null)
        {
            content = syntax.Syntax;
        }

        throw Context.Reply(HelpFormat, command, content);
    }
}

[CommandParent(typeof(HelpCommand))]
[CommandData("commands")]
internal class HelpCommandsCommand : Command
{
    public HelpCommandsCommand(CommandContext context) : base(context)
    {
    }

    private static readonly Translation CommandHelp = new("CommandHelp");

    public override UniTask ExecuteAsync()
    {
        throw Context.Reply(CommandHelp);
    }
}
