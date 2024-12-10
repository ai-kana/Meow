using System.Reflection;
using Microsoft.Extensions.Logging;
using Kronstadt.Core.Logging;

namespace Kronstadt.Core.Commands.Framework;

internal class CommandTypeData
{
    public readonly Type OwnerType;
    private readonly SubCommandData _SubCommands;
    private readonly ILogger _Logger;
    private readonly Assembly _Assembly;

    internal CommandTypeData(Type type, Assembly assembly)
    {
        OwnerType = type;
        _Assembly = assembly;
        _Logger = LoggerProvider.CreateLogger<CommandTypeData>();
        try
        {
            _SubCommands = BuildSubCommandTree();
        }
        catch (Exception exception)
        {
            _Logger.LogError(exception.ToString());
            _SubCommands = new(OwnerType);
        }
    }

    private SubCommandData RegisterSubCommand(SubCommandData owner, IEnumerable<Type> commandTypes, int i)
    {
        i++;
        foreach (Type command in commandTypes)
        {
            CommandParentAttribute? parent = command.GetCustomAttribute<CommandParentAttribute>();
            if (parent == null)
            {
                continue;
            }

            if (parent.Owner != owner.Type)
            {
                continue;
            }

            CommandDataAttribute? data = command.GetCustomAttribute<CommandDataAttribute>();
            if (data == null)
            {
                continue;
            }

            SubCommandData child = new(command);
            child.Switches.Add(data.Name);
            child.Switches.AddRange(data.Aliases);
            
            _Logger.LogInformation($"Registered sub command {i}::{command.FullName}");
            owner.Children.Add(RegisterSubCommand(child, commandTypes, i));
        }

        return owner;
    }

    private SubCommandData BuildSubCommandTree()
    {
        IEnumerable<Type> commands = _Assembly.GetTypes().Where(x => x.BaseType == typeof(Command));
        SubCommandData owner = new(OwnerType);
        return RegisterSubCommand(owner, commands, 0);
    }

    public Type GetCommand(IEnumerable<string> arguments, out int depth)
    {
        IEnumerator<string> enumerator = arguments.GetEnumerator();
        depth = 0;
        // move to first argument or return
        if (!enumerator.MoveNext() || !enumerator.MoveNext())
        {
            return OwnerType; 
        }

        SubCommandData current = _SubCommands;
        do
        {
            foreach (SubCommandData data in current.Children)
            {
                if (data.Switches.Contains(enumerator.Current, StringComparer.OrdinalIgnoreCase))
                {
                    current = data;
                    depth++;
                    break;
                }
            }
        }
        while (enumerator.MoveNext());

        return current.Type;
    }

    private class SubCommandData
    {
        public List<string> Switches;
        public Type Type;
        public List<SubCommandData> Children {get; private set;}

        public SubCommandData(Type type)
        {
            Children = new();
            Switches = new();
            Type = type;
        }
    }
}
