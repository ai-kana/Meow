using Cysharp.Threading.Tasks;
using Kronstadt.Core.Commands.Framework;
using Kronstadt.Core.Players;
using Kronstadt.Core.Translations;
using Command = Kronstadt.Core.Commands.Framework.Command;

namespace Kronstadt.Core.Commands.StaffCommands;

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
        throw Context.Reply("<[get | set | unset]>");
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

    public override UniTask ExecuteAsync()
    {
        Context.AssertPermission("flag");
        Context.AssertOnDuty();
        Context.AssertArguments(2);

        KronstadtPlayer player = Context.Parse<KronstadtPlayer>();
        Context.MoveNext();
        ushort flag = Context.Parse<ushort>();

        if (flag <= 0)
        {
            throw Context.Reply(TranslationList.GreaterThanZero);
        }

        if (!player.Quests.FlagExists(flag))
        {
            throw Context.Reply(TranslationList.FlagDoesNotExist, player.Name, flag);
        }
        
        player.Quests.TryGetFlag(flag, out short value);
        throw Context.Reply(TranslationList.FlagGet, flag, player.Name, value);
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
    
    public override UniTask ExecuteAsync()
    {
        Context.AssertPermission("flag");
        Context.AssertOnDuty();
        Context.AssertArguments(3);

        KronstadtPlayer player = Context.Parse<KronstadtPlayer>();
        Context.MoveNext();
        ushort flag = Context.Parse<ushort>();
        Context.MoveNext();
        short value = Context.Parse<short>();

        player.Quests.SetFlag(flag, value);
        throw Context.Reply(TranslationList.FlagSet, flag, player.Name, value);
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

    public override UniTask ExecuteAsync()
    {
        Context.AssertPermission("flag");
        Context.AssertOnDuty();
        Context.AssertArguments(2);

        KronstadtPlayer player = Context.Parse<KronstadtPlayer>();
        Context.MoveNext();
        ushort flag = Context.Parse<ushort>();

        if (!player.Quests.FlagExists(flag))
        {
            throw Context.Reply(TranslationList.FlagDoesNotExist, player.Name, flag);
        }
        
        player.Quests.RemoveFlag(flag);
        throw Context.Reply(TranslationList.FlagUnset, flag, player.Name);
    }
}
