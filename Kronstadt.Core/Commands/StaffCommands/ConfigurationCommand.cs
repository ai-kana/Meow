using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Kronstadt.Core.Commands.Framework;
using Kronstadt.Core.Configuration;

namespace Kronstadt.Core.Commands.StaffCommands;

[CommandData("configuration", "config")]
internal class ConfigurationCommand : Command
{
    public ConfigurationCommand(CommandContext context) : base(context)
    {
    }

    public override UniTask ExecuteAsync()
    {
        Context.AssertPermission("config");

        IConfigurationRoot root = (IConfigurationRoot)KronstadtHost.Configuration;
        root.Reload();

        ConfigurationEvents.OnConfigurationReloaded?.Invoke();

        throw Context.Reply("Reloaded configuration");
    }
}
