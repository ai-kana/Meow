using Cysharp.Threading.Tasks;
using Meow.Core.Binds;
using Meow.Core.Commands.Framework;
using Meow.Core.Players;
using Meow.Core.Translations;

namespace Meow.Core.Commands;

[CommandData("bind")]
[CommandSyntax("[<Switch: remove> <Params: pluginkey>] [<Params: command...>]")]
internal class BindCommand : Command
{
    public BindCommand(CommandContext context) : base(context)
    {
    }

    private static readonly Translation SetBind = new("SetBind");
    public override UniTask ExecuteAsync()
    {
        Context.AssertPlayer(out MeowPlayer caller);
        Context.AssertArguments(2);

        byte bindNumber = Context.Parse<byte>();
        if (bindNumber > 5)
        {
            throw Context.Reply(TranslationList.LessThanX, 5);
        }

        if (bindNumber == 0)
        {
            throw Context.Reply(TranslationList.GreaterThanZero);
        }

        Context.MoveNext();
        string command = Context.Form();
        BindManager.SetBind(caller, bindNumber, command);

        throw Context.Reply(SetBind, bindNumber, command);
    }
}

[CommandParent(typeof(BindCommand))]
[CommandData("remove")]
[CommandSyntax("[<Params: pluginkey>]")]
internal class BindRemoveCommand : Command
{
    public BindRemoveCommand(CommandContext context) : base(context)
    {
    }

    private static readonly Translation RemoveBind = new("RemoveBind");
    public override UniTask ExecuteAsync()
    {
        Context.AssertPlayer(out MeowPlayer caller);
        Context.AssertArguments(1);

        byte bindNumber = Context.Parse<byte>();
        if (bindNumber > 5)
        {
            throw Context.Reply(TranslationList.LessThanX, 5);
        }

        BindManager.RemoveBind(caller, bindNumber);

        throw Context.Reply(RemoveBind, bindNumber);
    }
}
