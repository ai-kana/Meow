using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Meow.Core.Commands.Framework;
using Meow.Core.Configuration;

namespace Meow.Core.Commands.StaffCommands;

[CommandData("configuration", "config")]
internal class ConfigurationCommand : Command
{
    public ConfigurationCommand(CommandContext context) : base(context)
    {
    }

    public override UniTask ExecuteAsync()
    {
        Context.AssertPermission("config");

        IConfigurationRoot root = (IConfigurationRoot)MeowHost.Configuration;
        root.Reload();

        ConfigurationEvents.OnConfigurationReloaded?.Invoke();

        throw Context.Reply("Reloaded configuration");
    }
}
