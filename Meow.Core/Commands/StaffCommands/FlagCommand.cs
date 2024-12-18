using Cysharp.Threading.Tasks;
using Meow.Core.Commands.Framework;
using Meow.Core.Players;
using Meow.Core.Translations;
using Command = Meow.Core.Commands.Framework.Command;

namespace Meow.Core.Commands.StaffCommands;

[CommandData("flag")]
[CommandSyntax("[<Switches: get, set, unset>]")]
internal class FlagCommand : Command
{
    public FlagCommand(CommandContext context) : base(context)
    {
    }

    public override UniTask ExecuteAsync()
    {
        Context.AssertPermission("flag");
        Context.AssertOnDuty();
        throw Context.Reply("[<Switches: get, set, unset>]");
    }
}

[CommandParent(typeof(FlagCommand))]
[CommandData("get")]
[CommandSyntax("[<Params: player>] [<Params: flag>]")]
internal class FlagGetCommand : Command
{
    public FlagGetCommand(CommandContext context) : base(context)
    {
    }

    private static readonly Translation FlagDoesNotExist = new("FlagDoesNotExist");
    private static readonly Translation FlagGet = new("FlagGet");

    public override UniTask ExecuteAsync()
    {
        Context.AssertPermission("flag");
        Context.AssertOnDuty();
        Context.AssertArguments(2);

        MeowPlayer player = Context.Parse<MeowPlayer>();
        Context.MoveNext();
        ushort flag = Context.Parse<ushort>();

        if (flag <= 0)
        {
            throw Context.Reply(TranslationList.GreaterThanZero);
        }

        if (!player.Quests.FlagExists(flag))
        {
            throw Context.Reply(FlagDoesNotExist, player.Name, flag);
        }
        
        player.Quests.TryGetFlag(flag, out short value);
        throw Context.Reply(FlagGet, flag, player.Name, value);
    }
}

[CommandParent(typeof(FlagCommand))]
[CommandData("set")]
[CommandSyntax("[<Params: player>] [<Params: flag>] [<Params: value>]")]
internal class FlagSetCommand : Command
{
    public FlagSetCommand(CommandContext context) : base(context)
    {
    }
    
    private static readonly Translation FlagSet = new("FlagSet");

    public override UniTask ExecuteAsync()
    {
        Context.AssertPermission("flag");
        Context.AssertOnDuty();
        Context.AssertArguments(3);

        MeowPlayer player = Context.Parse<MeowPlayer>();
        Context.MoveNext();
        ushort flag = Context.Parse<ushort>();
        Context.MoveNext();
        short value = Context.Parse<short>();

        player.Quests.SetFlag(flag, value);
        throw Context.Reply(FlagSet, flag, player.Name, value);
    }
}

[CommandParent(typeof(FlagCommand))]
[CommandData("unset")]
[CommandSyntax("[<Params: player>] [<Params: flag>]")]
internal class FlagUnsetCommand : Command
{
    public FlagUnsetCommand(CommandContext context) : base(context)
    {
    }

    private static readonly Translation FlagUnset = new("FlagUnset");
    private static readonly Translation FlagDoesNotExist = new("FlagDoesNotExist");

    public override UniTask ExecuteAsync()
    {
        Context.AssertPermission("flag");
        Context.AssertOnDuty();
        Context.AssertArguments(2);

        MeowPlayer player = Context.Parse<MeowPlayer>();
        Context.MoveNext();
        ushort flag = Context.Parse<ushort>();

        if (!player.Quests.FlagExists(flag))
        {
            throw Context.Reply(FlagDoesNotExist, player.Name, flag);
        }
        
        player.Quests.RemoveFlag(flag);
        throw Context.Reply(FlagUnset, flag, player.Name);
    }
}
