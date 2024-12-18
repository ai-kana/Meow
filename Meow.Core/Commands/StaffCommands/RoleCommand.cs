using Cysharp.Threading.Tasks;
using Meow.Core.Commands.Framework;
using Meow.Core.Formatting;
using Meow.Core.Players;
using Meow.Core.Roles;
using Meow.Core.Translations;

namespace Meow.Core.Commands.StaffCommands;

[CommandData("role")]
[CommandSyntax("[<Switches: add, remove, list>]")]
internal class RoleCommand : Command
{
    public RoleCommand(CommandContext context) : base(context)
    {
    }

    public override UniTask ExecuteAsync()
    {
        Context.AssertPermission("role");
        throw Context.Reply("<add, a | remove, r | list>");
    }
}

[CommandParent(typeof(RoleCommand))]
[CommandData("add", "a")]
[CommandSyntax("[<Params: player>] [<Params: role>]")]
internal class RoleAddCommand : Command
{
    public RoleAddCommand(CommandContext context) : base(context)
    {
    }

    private static readonly Translation AddedRole = new("AddedRole");

    public override UniTask ExecuteAsync()
    {
        Context.AssertPermission("role");
        Context.AssertArguments(2);

        MeowPlayer player = Context.Parse<MeowPlayer>();
        Context.MoveNext();
        Role role = Context.Parse<Role>();

        player.Roles.AddRole(role.Id);
        throw Context.Reply(AddedRole, player.Name, role.Id);
    }
}

[CommandParent(typeof(RoleCommand))]
[CommandData("remove", "r")]
[CommandSyntax("[<Params: player>] [<Params: role>]")]
internal class RoleRemoveCommand : Command
{
    public RoleRemoveCommand(CommandContext context) : base(context)
    {
    }
    private static readonly Translation RemovedRole = new("RemovedRole");
    private static readonly Translation DoesNotHaveRole = new("DoesNotHaveRole");

    public override UniTask ExecuteAsync()
    {
        Context.AssertPermission("role");
        Context.AssertArguments(2);

        MeowPlayer player = Context.Parse<MeowPlayer>();
        Context.MoveNext();
        Role role = Context.Parse<Role>();

        if (!player.Roles.HasRole(role.Id))
        {
            throw Context.Reply(DoesNotHaveRole, player.Name, role.Id);
        }

        player.Roles.AddRole(role.Id);
        throw Context.Reply(RemovedRole, player.Name, role.Id);
    }
}

[CommandParent(typeof(RoleCommand))]
[CommandData("list")]
[CommandSyntax("[<Params: player>]")]
internal class RoleListCommand : Command {
    public RoleListCommand(CommandContext context) : base(context)
    {
    }

    private static readonly Translation RoleList = new("RoleList");
    private static readonly Translation RoleHasRole = new("RoleHasRole");

    public override UniTask ExecuteAsync()
    {
        Context.AssertPermission("role");

        if (Context.HasExactArguments(0))
        {
            throw Context.Reply(RoleList, Formatter.FormatList(RoleManager.Roles.Select(x => x.Id), ", "));
        }

        MeowPlayer player = Context.Parse<MeowPlayer>();

        HashSet<Role> roles = RoleManager.GetRoles(player.Roles.Roles);
        throw Context.Reply(RoleHasRole, player.Name, Formatter.FormatList(roles.Select(x => x.Id), ", "));
    }
}
