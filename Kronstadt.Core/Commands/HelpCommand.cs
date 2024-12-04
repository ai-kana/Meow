using System.Reflection;
using Cysharp.Threading.Tasks;
using Kronstadt.Core.Commands.Framework;
using Kronstadt.Core.Translations;

namespace Kronstadt.Core.Commands;

[CommandData("help")]
[CommandSyntax("<[command] | commands>")]
internal class HelpCommand : Command
{
    public HelpCommand(CommandContext context) : base(context)
    {
    }

    private readonly Translation HelpFormat = new("HelpFormat", "{0}: {1}");

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

    public override UniTask ExecuteAsync()
    {
        throw Context.Reply(
                "When using /help <command> it will reply with the possible options. Options are wrapped in <> " 
                + "An argument wrapped in [] is a value that you need to supply. "
                + "A ? symbol means the argument is optional. "
                + "A | symbol means there is different options between them. "
                + "A , is an alias of an option");
    }
}
