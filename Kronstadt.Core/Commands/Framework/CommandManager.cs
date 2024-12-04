using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.Extensions.Logging;
using SDG.Unturned;
using Kronstadt.Core.Logging;
using Kronstadt.Core.Players;
using Kronstadt.Core.Translations;

namespace Kronstadt.Core.Commands.Framework;

internal class CommandManager
{
    private static ConcurrentDictionary<string, CommandTypeData> _CommandTypes;
    private static readonly ILogger _Logger;

    static CommandManager()
    {
        _Logger = LoggerProvider.CreateLogger<CommandManager>();
        CommandWindow.onCommandWindowInputted += OnInput;
        _CommandTypes = new();
    }

    private static void TryRegisterCommand(string name, CommandTypeData type, bool isAlias)
    {
        string fixedName = name.ToLower();
        if (_CommandTypes.TryAdd(fixedName, type))
        {
            _Logger.LogInformation(isAlias ? $"Register command alias {fixedName}" : $"Registered command {fixedName}");
            return;
        }

        if (_CommandTypes.TryGetValue(name, out CommandTypeData owner))
        {
            _Logger.LogWarning($"Tried to register duplicate command name from {type.OwnerType.FullName}; originally registered from {owner.OwnerType.FullName}");
            return;
        }

        _Logger.LogWarning($"Something failed trying to register command alias");
    }

    public static void RegisterCommandTypes(Assembly assembly)
    {
        Type[] types = assembly.GetTypes();
        foreach (Type type in types)
        {
            if (type.BaseType != typeof(Command))
            {
                continue;
            }

            CommandDataAttribute? commandData = type.GetCustomAttribute<CommandDataAttribute>();
            if (commandData == null)
            {
                continue;
            }

            CommandParentAttribute? parent = type.GetCustomAttribute<CommandParentAttribute>();
            if (parent != null)
            {
                continue;
            }

            CommandTypeData data = new(type, assembly);
            TryRegisterCommand(commandData.Name, data, false);
            foreach (string name in commandData.Aliases)
            {
                TryRegisterCommand(name, data, true);
            }
        }
    }

    public static Type? GetCommandType(IEnumerable<string> arguments)
    {
        string name = arguments.First();
        if (!_CommandTypes.TryGetValue(name, out CommandTypeData typeData))
        {
            return null;
        }

        Type commandType = typeData.GetCommand(arguments, out int depth);

        return commandType;
    }

    private static Type? GetCommandType(IEnumerable<string> arguments, out int depth)
    {
        string name = arguments.First();
        depth = 0;
        if (!_CommandTypes.TryGetValue(name, out CommandTypeData typeData))
        {
            return null;
        }

        Type commandType = typeData.GetCommand(arguments, out depth);

        return commandType;
    }

    public static async void ExecuteCommand(string commandText, IPlayer caller)
    {
        CommandTokenizer parser = new(commandText);
        IEnumerable<string> arguments = parser.Parse();

        Type? type = GetCommandType(arguments, out int depth);
        if (type == null)
        {
            caller.SendMessage(TranslationList.NoCommandFound, arguments.First());
            return;
        }
        arguments = arguments.Skip(1 + depth);

        CommandContext context = new(type, arguments, caller);
        Command command = (Command)Activator.CreateInstance(type, args: context);
        _Logger.LogInformation($"Executing command [{caller.LogName}]: {commandText}");
        try
        {
            await command.ExecuteAsync();
        }
        catch (CommandExitedException)
        {
        }
        catch (UserMessageException message)
        {
            caller.SendMessage(message.PlayerMessage);
        }
        catch (Exception exception)
        {
            _Logger.LogError(exception, "Failed to execute command");
            _Logger.LogError(exception.ToString());
        }
    }

    private static void OnInput(string message, ref bool shouldExecuteCommand)
    {
        shouldExecuteCommand = false;
        ExecuteCommand(message, new ConsolePlayer());
    }
}
